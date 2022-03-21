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

namespace ASC.Web.Files.Services.WCFService.FileOperations;

internal class FileDownloadOperationData<T> : FileOperationData<T>
{
    public Dictionary<T, string> FilesDownload { get; }
    public IDictionary<string, StringValues> Headers { get; }

    public FileDownloadOperationData(Dictionary<T, string> folders, Dictionary<T, string> files, Tenant tenant, IDictionary<string, StringValues> headers, bool holdResult = true)
        : base(folders.Select(f => f.Key).ToList(), files.Select(f => f.Key).ToList(), tenant, holdResult)
    {
        FilesDownload = files;
        Headers = headers;
    }
}

[Transient]
class FileDownloadOperation : ComposeFileOperation<FileDownloadOperationData<string>, FileDownloadOperationData<int>>
{
    public FileDownloadOperation(IServiceProvider serviceProvider, TempStream tempStream, FileOperation<FileDownloadOperationData<string>, string> f1, FileOperation<FileDownloadOperationData<int>, int> f2)
        : base(serviceProvider, f1, f2)
    {
        _tempStream = tempStream;
    }

    public override FileOperationType OperationType => FileOperationType.Download;

    private readonly TempStream _tempStream;

    public override async Task RunJobAsync(DistributedTask distributedTask, CancellationToken cancellationToken)
    {
        await base.RunJobAsync(distributedTask, cancellationToken);

        using var scope = ThirdPartyOperation.CreateScope();
        var scopeClass = scope.ServiceProvider.GetService<FileDownloadOperationScope>();
        var (globalStore, filesLinkUtility, _, _, _) = scopeClass;
        var stream = _tempStream.Create();

        await (ThirdPartyOperation as FileDownloadOperation<string>).CompressToZipAsync(stream, scope);
        await (DaoOperation as FileDownloadOperation<int>).CompressToZipAsync(stream, scope);

        if (stream != null)
        {
            var archiveExtension = "";

            using (var zip = scope.ServiceProvider.GetService<CompressToArchive>())
            {
                archiveExtension = zip.ArchiveExtension;
            }

            stream.Position = 0;
            string fileName = FileConstant.DownloadTitle + archiveExtension;
            var store = globalStore.GetStore();
            var path = string.Format(@"{0}\{1}", ((IAccount)Thread.CurrentPrincipal.Identity).ID, fileName);

            if (await store.IsFileAsync(FileConstant.StorageDomainTmp, path))
            {
                await store.DeleteAsync(FileConstant.StorageDomainTmp, path);
            }

            await store.SaveAsync(
                FileConstant.StorageDomainTmp,
                path,
                stream,
                MimeMapping.GetMimeMapping(path),
                "attachment; filename=\"" + fileName + "\"");

            Result = $"{filesLinkUtility.FileHandlerPath}?{FilesLinkUtility.Action}=bulk&ext={archiveExtension}";
        }

        FillDistributedTask();
        TaskInfo.PublishChanges();
    }

    public override void PublishChanges(DistributedTask task)
    {
        var thirdpartyTask = ThirdPartyOperation.GetDistributedTask();
        var daoTask = DaoOperation.GetDistributedTask();

        var error1 = thirdpartyTask.GetProperty<string>(Err);
        var error2 = daoTask.GetProperty<string>(Err);

        if (!string.IsNullOrEmpty(error1))
        {
            Error = error1;
        }
        else if (!string.IsNullOrEmpty(error2))
        {
            Error = error2;
        }

        SuccessProcessed = thirdpartyTask.GetProperty<int>(Process) + daoTask.GetProperty<int>(Process);

        var progressSteps = ThirdPartyOperation.Total + DaoOperation.Total + 1;

        var progress = (int)(SuccessProcessed / (double)progressSteps * 100);

        base.FillDistributedTask();

        TaskInfo.SetProperty(Progress, progress);
        TaskInfo.PublishChanges();
    }
}

class FileDownloadOperation<T> : FileOperation<FileDownloadOperationData<T>, T>
{
    private readonly Dictionary<T, string> _files;
    private readonly IDictionary<string, StringValues> _headers;
    private ItemNameValueCollection<T> _entriesPathId;
    public override FileOperationType OperationType => FileOperationType.Download;

    public FileDownloadOperation(IServiceProvider serviceProvider, FileDownloadOperationData<T> fileDownloadOperationData)
        : base(serviceProvider, fileDownloadOperationData)
    {
        _files = fileDownloadOperationData.FilesDownload;
        _headers = fileDownloadOperationData.Headers;
    }

    protected override async Task DoAsync(IServiceScope scope)
    {
        if (Files.Count == 0 && Folders.Count == 0)
        {
            return;
        }

        _entriesPathId = await GetEntriesPathIdAsync(scope);
        if (_entriesPathId == null || _entriesPathId.Count == 0)
        {
            if (Files.Count > 0)
            {
                throw new FileNotFoundException(FilesCommonResource.ErrorMassage_FileNotFound);
            }

            throw new DirectoryNotFoundException(FilesCommonResource.ErrorMassage_FolderNotFound);
        }

        ReplaceLongPath(_entriesPathId);

        Total = _entriesPathId.Count;

        TaskInfo.PublishChanges();
    }

    private async Task<ItemNameValueCollection<T>> ExecPathFromFileAsync(IServiceScope scope, File<T> file, string path)
    {
        var fileMarker = scope.ServiceProvider.GetService<FileMarker>();
        await fileMarker.RemoveMarkAsNewAsync(file);

        var title = file.Title;

        if (_files.TryGetValue(file.ID, out var convertToExt))
        {
            if (!string.IsNullOrEmpty(convertToExt))
            {
                title = FileUtility.ReplaceFileExtension(title, convertToExt);
            }
        }

        var entriesPathId = new ItemNameValueCollection<T>();
        entriesPathId.Add(path + title, file.ID);

        return entriesPathId;
    }

    private async Task<ItemNameValueCollection<T>> GetEntriesPathIdAsync(IServiceScope scope)
    {
        var fileMarker = scope.ServiceProvider.GetService<FileMarker>();
        var entriesPathId = new ItemNameValueCollection<T>();
        if (0 < Files.Count)
        {
            var files = await FileDao.GetFilesAsync(Files).ToListAsync();
            files = (await FilesSecurity.FilterReadAsync(files)).ToList();

            foreach (var file in files)
            {
                entriesPathId.Add(await ExecPathFromFileAsync(scope, file, string.Empty));
            }
        }
        if (0 < Folders.Count)
        {
            var filteredFolders = await FilesSecurity.FilterReadAsync(await FolderDao.GetFoldersAsync(Files).ToListAsync());

            foreach (var folder in filteredFolders)
            {
                await fileMarker.RemoveMarkAsNewAsync(folder);
            }

            var filesInFolder = await GetFilesInFoldersAsync(scope, Folders, string.Empty);
            entriesPathId.Add(filesInFolder);
        }

        return entriesPathId;
    }

    private async Task<ItemNameValueCollection<T>> GetFilesInFoldersAsync(IServiceScope scope, IEnumerable<T> folderIds, string path)
    {
        var fileMarker = scope.ServiceProvider.GetService<FileMarker>();

        CancellationToken.ThrowIfCancellationRequested();

        var entriesPathId = new ItemNameValueCollection<T>();
        foreach (var folderId in folderIds)
        {
            CancellationToken.ThrowIfCancellationRequested();

            var folder = await FolderDao.GetFolderAsync(folderId);
            if (folder == null || !await FilesSecurity.CanReadAsync(folder))
            {
                continue;
            }

            var folderPath = path + folder.Title + "/";

            var files = await FileDao.GetFilesAsync(folder.ID, null, FilterType.None, false, Guid.Empty, string.Empty, true).ToListAsync();
            var filteredFiles = await FilesSecurity.FilterReadAsync(files);
            files = filteredFiles.ToList();

            foreach (var file in filteredFiles)
            {
                entriesPathId.Add(await ExecPathFromFileAsync(scope, file, folderPath));
            }

            await fileMarker.RemoveMarkAsNewAsync(folder);

            var nestedFolders = await FolderDao.GetFoldersAsync(folder.ID).ToListAsync();
            var filteredNestedFolders = await FilesSecurity.FilterReadAsync(nestedFolders);
            nestedFolders = filteredNestedFolders.ToList();
            if (files.Count == 0 && nestedFolders.Count == 0)
            {
                entriesPathId.Add(folderPath, default(T));
            }

            var filesInFolder = await GetFilesInFoldersAsync(scope, nestedFolders.ConvertAll(f => f.ID), folderPath);
            entriesPathId.Add(filesInFolder);
        }

        return entriesPathId;
    }

    internal async Task CompressToZipAsync(Stream stream, IServiceScope scope)
    {
        if (_entriesPathId == null)
        {
            return;
        }

        var scopeClass = scope.ServiceProvider.GetService<FileDownloadOperationScope>();
        var (_, _, _, fileConverter, filesMessageService) = scopeClass;
        var FileDao = scope.ServiceProvider.GetService<IFileDao<T>>();

        using (var compressTo = scope.ServiceProvider.GetService<CompressToArchive>())
        {
            compressTo.SetStream(stream);

            foreach (var path in _entriesPathId.AllKeys)
            {
                var counter = 0;
                foreach (var entryId in _entriesPathId[path])
                {
                    if (CancellationToken.IsCancellationRequested)
                    {
                        CancellationToken.ThrowIfCancellationRequested();
                    }

                    var newtitle = path;

                    File<T> file = null;
                    var convertToExt = string.Empty;

                    if (!Equals(entryId, default(T)))
                    {
                        await FileDao.InvalidateCacheAsync(entryId);
                        file = await FileDao.GetFileAsync(entryId);

                        if (file == null)
                        {
                            Error = FilesCommonResource.ErrorMassage_FileNotFound;
                            continue;
                        }

                        if (_files.TryGetValue(file.ID, out convertToExt))
                        {
                            if (!string.IsNullOrEmpty(convertToExt))
                            {
                                newtitle = FileUtility.ReplaceFileExtension(path, convertToExt);
                            }
                        }
                    }

                    if (0 < counter)
                    {
                        var suffix = " (" + counter + ")";

                        if (!Equals(entryId, default(T)))
                        {
                            newtitle = newtitle.IndexOf('.') > 0 ? newtitle.Insert(newtitle.LastIndexOf('.'), suffix) : newtitle + suffix;
                        }
                        else
                        {
                            break;
                        }
                    }

                    compressTo.CreateEntry(newtitle);

                    if (!Equals(entryId, default(T)) && file != null)
                    {
                        try
                        {
                            if (fileConverter.EnableConvert(file, convertToExt))
                            {
                                //Take from converter
                                using (var readStream = await fileConverter.ExecAsync(file, convertToExt))
                                {
                                    compressTo.PutStream(readStream);

                                    if (!string.IsNullOrEmpty(convertToExt))
                                    {
                                        filesMessageService.Send(file, _headers, MessageAction.FileDownloadedAs, file.Title, convertToExt);
                                    }
                                    else
                                    {
                                        filesMessageService.Send(file, _headers, MessageAction.FileDownloaded, file.Title);
                                    }
                                }
                            }
                            else
                            {
                                using (var readStream = await FileDao.GetFileStreamAsync(file))
                                {
                                    compressTo.PutStream(readStream);

                                    filesMessageService.Send(file, _headers, MessageAction.FileDownloaded, file.Title);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Error = ex.Message;
                            Logger.Error(Error, ex);
                        }
                    }
                    else
                    {
                        compressTo.PutNextEntry();
                    }

                    compressTo.CloseEntry();
                    counter++;
                }

                ProgressStep();
            }
        }
    }

    private void ReplaceLongPath(ItemNameValueCollection<T> entriesPathId)
    {
        foreach (var path in new List<string>(entriesPathId.AllKeys))
        {
            CancellationToken.ThrowIfCancellationRequested();

            if (200 >= path.Length || 0 >= path.IndexOf('/'))
            {
                continue;
            }

            var ids = entriesPathId[path];
            entriesPathId.Remove(path);

            var newtitle = "LONG_FOLDER_NAME" + path.Substring(path.LastIndexOf('/'));
            entriesPathId.Add(newtitle, ids);
        }
    }
}

internal class ItemNameValueCollection<T>
{
    private readonly Dictionary<string, List<T>> _dic = new Dictionary<string, List<T>>();


    public IEnumerable<string> AllKeys => _dic.Keys;

    public IEnumerable<T> this[string name] => _dic[name].ToArray();

    public int Count => _dic.Count;

    public void Add(string name, T value)
    {
        if (!_dic.ContainsKey(name))
        {
            _dic.Add(name, new List<T>());
        }

        _dic[name].Add(value);
    }

    public void Add(ItemNameValueCollection<T> collection)
    {
        foreach (var key in collection.AllKeys)
        {
            foreach (var value in collection[key])
            {
                Add(key, value);
            }
        }
    }

    public void Add(string name, IEnumerable<T> values)
    {
        if (!_dic.ContainsKey(name))
        {
            _dic.Add(name, new List<T>());
        }

        _dic[name].AddRange(values);
    }

    public void Remove(string name)
    {
        _dic.Remove(name);
    }
}

[Scope]
public class FileDownloadOperationScope
{
    private readonly GlobalStore _globalStore;
    private readonly FilesLinkUtility _filesLinkUtility;
    private readonly SetupInfo _setupInfo;
    private readonly FileConverter _fileConverter;
    private readonly FilesMessageService _filesMessageService;

    public FileDownloadOperationScope(
        GlobalStore globalStore,
        FilesLinkUtility filesLinkUtility,
        SetupInfo setupInfo,
        FileConverter fileConverter,
        FilesMessageService filesMessageService)
    {
        _globalStore = globalStore;
        _filesLinkUtility = filesLinkUtility;
        _setupInfo = setupInfo;
        _fileConverter = fileConverter;
        _filesMessageService = filesMessageService;
    }

    public void Deconstruct(out GlobalStore globalStore, out FilesLinkUtility filesLinkUtility, out SetupInfo setupInfo, out FileConverter fileConverter, out FilesMessageService filesMessageService)
    {
        globalStore = _globalStore;
        filesLinkUtility = _filesLinkUtility;
        setupInfo = _setupInfo;
        fileConverter = _fileConverter;
        filesMessageService = _filesMessageService;
    }
}
