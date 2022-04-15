// (c) Copyright Ascensio System SIA 2010-2022
//
// This program is a free software product.
// You can redistribute it and/or modify it under the terms
// of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
// Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
// to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of
// any third-party rights.
//
// This program is distributed WITHOUT ANY WARRANTY, without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see
// the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
//
// You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
//
// The  interactive user interfaces in modified source and object code versions of the Program must
// display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
//
// Pursuant to Section 7(b) of the License you must retain the original Product logo when
// distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under
// trademark law for use of our trademarks.
//
// All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
// content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
// International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode

namespace ASC.Data.Backup.Tasks;

[Scope]
public class RestorePortalTask : PortalTaskBase
{
    public bool ReplaceDate { get; set; }
    public bool Dump { get; set; }
    public string BackupFilePath { get; private set; }
    public string UpgradesPath { get; private set; }
    public bool UnblockPortalAfterCompleted { get; set; }

    private ColumnMapper _columnMapper;
    private readonly CoreBaseSettings _coreBaseSettings;
    private readonly LicenseReader _licenseReader;
    private readonly TenantManager _tenantManager;
    private readonly AscCacheNotify _ascCacheNotify;
    private readonly IOptionsMonitor<ILog> _options;

    public RestorePortalTask(
        DbFactory dbFactory,
        IOptionsMonitor<ILog> options,
        StorageFactory storageFactory,
        StorageFactoryConfig storageFactoryConfig,
        CoreBaseSettings coreBaseSettings,
        LicenseReader licenseReader,
        TenantManager tenantManager,
        AscCacheNotify ascCacheNotify,
        ModuleProvider moduleProvider)
        : base(dbFactory, options, storageFactory, storageFactoryConfig, moduleProvider)
    {
        _coreBaseSettings = coreBaseSettings;
        _licenseReader = licenseReader;
        _tenantManager = tenantManager;
        _ascCacheNotify = ascCacheNotify;
        _options = options;
    }

    public void Init(string toConfigPath, string fromFilePath, int tenantId = -1, ColumnMapper columnMapper = null, string upgradesPath = null)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(fromFilePath);

        if (!File.Exists(fromFilePath))
        {
            throw new FileNotFoundException("file not found at given path");
        }

        BackupFilePath = fromFilePath;
        UpgradesPath = upgradesPath;
        _columnMapper = columnMapper ?? new ColumnMapper();
        Init(tenantId, toConfigPath);
    }

    public override void RunJob()
    {
        Logger.Debug("begin restore portal");

        Logger.Debug("begin restore data");

        using (var dataReader = new ZipReadOperator(BackupFilePath))
        {
            using (var entry = dataReader.GetEntry(KeyHelper.GetDumpKey()))
            {
                Dump = entry != null && _coreBaseSettings.Standalone;
            }

            if (Dump)
            {
                RestoreFromDump(dataReader);
            }
            else
            {
                var modulesToProcess = GetModulesToProcess().ToList();
                SetStepsCount(ProcessStorage ? modulesToProcess.Count + 1 : modulesToProcess.Count);

                foreach (var module in modulesToProcess)
                {
                    var restoreTask = new RestoreDbModuleTask(_options, module, dataReader, _columnMapper, DbFactory, ReplaceDate, Dump, StorageFactory, StorageFactoryConfig, ModuleProvider);
                    restoreTask.ProgressChanged += (sender, args) => SetCurrentStepProgress(args.Progress);

                    foreach (var tableName in _ignoredTables)
                    {
                        restoreTask.IgnoreTable(tableName);
                    }

                    restoreTask.RunJob();
                }
            }

            Logger.Debug("end restore data");

            if (ProcessStorage)
            {
                if (_coreBaseSettings.Standalone)
                {
                    Logger.Debug("clear cache");
                    _ascCacheNotify.ClearCache();
                }

                DoRestoreStorage(dataReader);
            }

            if (UnblockPortalAfterCompleted)
            {
                SetTenantActive(_columnMapper.GetTenantMapping());
            }
        }

        if (_coreBaseSettings.Standalone)
        {
            Logger.Debug("refresh license");
            try
            {
                _licenseReader.RejectLicense();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            Logger.Debug("clear cache");
            _ascCacheNotify.ClearCache();
        }

        Logger.Debug("end restore portal");
    }

    private void RestoreFromDump(IDataReadOperator dataReader)
    {
        var keyBase = KeyHelper.GetDatabaseSchema();
        var keys = dataReader.GetEntries(keyBase).Select(r => Path.GetFileName(r)).ToList();
        var upgrades = new List<string>();

        if (!string.IsNullOrEmpty(UpgradesPath) && Directory.Exists(UpgradesPath))
        {
            upgrades = Directory.GetFiles(UpgradesPath).ToList();
        }

        var stepscount = keys.Count * 2 + upgrades.Count;

        SetStepsCount(ProcessStorage ? stepscount + 1 : stepscount);

        if (ProcessStorage)
        {
            var storageModules = StorageFactoryConfig.GetModuleList(ConfigPath).Where(IsStorageModuleAllowed);
            var tenants = _tenantManager.GetTenants(false);

            stepscount += storageModules.Count() * tenants.Count;

            SetStepsCount(stepscount + 1);

            DoDeleteStorage(storageModules, tenants);
        }
        else
        {
            SetStepsCount(stepscount);
        }

        for (var i = 0; i < keys.Count; i += TasksLimit)
        {
            var tasks = new List<Task>(TasksLimit * 2);

            for (var j = 0; j < TasksLimit && i + j < keys.Count; j++)
            {
                var key1 = Path.Combine(KeyHelper.GetDatabaseSchema(), keys[i + j]);
                tasks.Add(RestoreFromDumpFile(dataReader, key1).ContinueWith(r => RestoreFromDumpFile(dataReader, KeyHelper.GetDatabaseData(key1.Substring(keyBase.Length + 1)))));
            }

            Task.WaitAll(tasks.ToArray());
        }

        var comparer = new SqlComparer();
        foreach (var u in upgrades.OrderBy(Path.GetFileName, comparer))
        {
            RunMysqlFile(u, true);
            SetStepCompleted();
        }
    }

    private async Task RestoreFromDumpFile(IDataReadOperator dataReader, string fileName)
    {
        Logger.DebugFormat("Restore from {0}", fileName);
        using (var stream = dataReader.GetEntry(fileName))
        {
            await RunMysqlFile(stream);
        }
        SetStepCompleted();
    }

    private class SqlComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y)
            {
                return 0;
            }

            if (!string.IsNullOrEmpty(x))
            {
                var splittedX = x.Split('.');
                if (splittedX.Length <= 2)
                {
                    return -1;
                }

                if (splittedX[1] == "upgrade")
                {
                    return 1;
                }

                if (splittedX[1].StartsWith("upgrade") && !string.IsNullOrEmpty(y))
                {
                    var splittedY = y.Split('.');
                    if (splittedY.Length <= 2)
                    {
                        return 1;
                    }

                    if (splittedY[1] == "upgrade")
                    {
                        return -1;
                    }

                    if (splittedY[1].StartsWith("upgrade"))
                    {
                        return string.Compare(x, y, StringComparison.Ordinal);
                    }

                    return -1;
                }

                return -1;
            }

            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }

    private void DoRestoreStorage(IDataReadOperator dataReader)
    {
        Logger.Debug("begin restore storage");

        var fileGroups = GetFilesToProcess(dataReader).GroupBy(file => file.Module).ToList();
        var groupsProcessed = 0;
        foreach (var group in fileGroups)
        {
            foreach (var file in group)
            {
                var storage = StorageFactory.GetStorage(ConfigPath, Dump ? file.Tenant.ToString() : _columnMapper.GetTenantMapping().ToString(), group.Key);
                var quotaController = storage.QuotaController;
                storage.SetQuotaController(null);

                try
                {
                    var adjustedPath = file.Path;
                    var module = ModuleProvider.GetByStorageModule(file.Module, file.Domain);
                    if (module == null || module.TryAdjustFilePath(Dump, _columnMapper, ref adjustedPath))
                    {
                        var key = file.GetZipKey();
                        if (Dump)
                        {
                            key = CrossPlatform.PathCombine(KeyHelper.GetStorage(), key);
                        }
                        using var stream = dataReader.GetEntry(key);
                        try
                        {
                            storage.SaveAsync(file.Domain, adjustedPath, module != null ? module.PrepareData(key, stream, _columnMapper) : stream).Wait();
                        }
                        catch (Exception error)
                        {
                            Logger.WarnFormat("can't restore file ({0}:{1}): {2}", file.Module, file.Path, error);
                        }
                    }
                }
                finally
                {
                    if (quotaController != null)
                    {
                        storage.SetQuotaController(quotaController);
                    }
                }
            }

            SetCurrentStepProgress((int)(++groupsProcessed * 100 / (double)fileGroups.Count));
        }

        if (fileGroups.Count == 0)
        {
            SetStepCompleted();
        }

        Logger.Debug("end restore storage");
    }

    private void DoDeleteStorage(IEnumerable<string> storageModules, IEnumerable<Tenant> tenants)
    {
        Logger.Debug("begin delete storage");

        foreach (var tenant in tenants)
        {
            foreach (var module in storageModules)
            {
                var storage = StorageFactory.GetStorage(ConfigPath, tenant.Id.ToString(), module);
                var domains = StorageFactoryConfig.GetDomainList(ConfigPath, module).ToList();

                domains.Add(string.Empty); //instead storage.DeleteFiles("\\", "*.*", true);

                foreach (var domain in domains)
                {
                    ActionInvoker.Try(
                        state =>
                        {
                            if (storage.IsDirectoryAsync((string)state).Result)
                            {
                                storage.DeleteFilesAsync((string)state, "\\", "*.*", true).Wait();
                            }
                        },
                        domain,
                        5,
                        onFailure: error => Logger.WarnFormat("Can't delete files for domain {0}: \r\n{1}", domain, error)
                    );
                }

                SetStepCompleted();
            }
        }

        Logger.Debug("end delete storage");
    }

    private IEnumerable<BackupFileInfo> GetFilesToProcess(IDataReadOperator dataReader)
    {
        using var stream = dataReader.GetEntry(KeyHelper.GetStorageRestoreInfoZipKey());
        if (stream == null)
        {
            return Enumerable.Empty<BackupFileInfo>();
        }

        var restoreInfo = XElement.Load(new StreamReader(stream));

        return restoreInfo.Elements("file").Select(BackupFileInfo.FromXElement).ToList();
    }

    private void SetTenantActive(int tenantId)
    {
        using var connection = DbFactory.OpenConnection();
        var commandText = string.Format(
            "update tenants_tenants " +
            "set " +
            "  status={0}, " +
            "  last_modified='{1}', " +
            "  statuschanged='{1}' " +
            "where id = '{2}'",
            (int)TenantStatus.Active,
            DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            tenantId);

        var command = connection.CreateCommand().WithTimeout(120);
        command.CommandText = commandText;
        command.ExecuteNonQuery();
    }
}
