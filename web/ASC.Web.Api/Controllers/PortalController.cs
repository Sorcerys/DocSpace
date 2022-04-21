﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using System.Web;

using ASC.Api.Core;
using ASC.Common;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Notify.Push;
using ASC.Core.Common.Settings;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Api.Models;
using ASC.Web.Api.Routing;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.PublicResources;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.Utility;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Api.Controllers
{
    [Scope]
    [DefaultRoute]
    [ApiController]
    public class PortalController : ControllerBase
    {
        private readonly ApiSystemHelper _apiSystemHelper;
        private readonly CoreSettings _coreSettings;
        private readonly StudioNotifyService _studioNotifyService;
        private readonly MessageService _messageService;
        private readonly MessageTarget _messageTarget;
        private readonly DisplayUserSettingsHelper _displayUserSettingsHelper;
        private readonly PermissionContext _permissionContext;

        private Tenant Tenant { get { return ApiContext.Tenant; } }

        private ApiContext ApiContext { get; }
        private UserManager UserManager { get; }
        private TenantManager TenantManager { get; }
        private PaymentManager PaymentManager { get; }
        private CommonLinkUtility CommonLinkUtility { get; }
        private UrlShortener UrlShortener { get; }
        private AuthContext AuthContext { get; }
        private WebItemSecurity WebItemSecurity { get; }
        private SecurityContext SecurityContext { get; }
        private SettingsManager SettingsManager { get; }
        private IMobileAppInstallRegistrator MobileAppInstallRegistrator { get; }
        private IConfiguration Configuration { get; set; }
        private CoreBaseSettings CoreBaseSettings { get; }
        private LicenseReader LicenseReader { get; }
        private SetupInfo SetupInfo { get; }
        private DocumentServiceLicense DocumentServiceLicense { get; }
        private TenantExtra TenantExtra { get; set; }
        public ILog Log { get; }
        public IHttpClientFactory ClientFactory { get; }


        public PortalController(
            IOptionsMonitor<ILog> options,
            ApiContext apiContext,
            UserManager userManager,
            TenantManager tenantManager,
            PaymentManager paymentManager,
            CommonLinkUtility commonLinkUtility,
            UrlShortener urlShortener,
            AuthContext authContext,
            WebItemSecurity webItemSecurity,
            SecurityContext securityContext,
            SettingsManager settingsManager,
            IMobileAppInstallRegistrator mobileAppInstallRegistrator,
            TenantExtra tenantExtra,
            IConfiguration configuration,
            CoreBaseSettings coreBaseSettings,
            LicenseReader licenseReader,
            SetupInfo setupInfo,
            DocumentServiceLicense documentServiceLicense,
            IHttpClientFactory clientFactory,
            ApiSystemHelper apiSystemHelper,
            CoreSettings coreSettings,
            PermissionContext permissionContext,
            StudioNotifyService studioNotifyService,
            MessageService messageService,
            MessageTarget messageTarget,
            DisplayUserSettingsHelper displayUserSettingsHelper
            )
        {
            Log = options.CurrentValue;
            ApiContext = apiContext;
            UserManager = userManager;
            TenantManager = tenantManager;
            PaymentManager = paymentManager;
            CommonLinkUtility = commonLinkUtility;
            UrlShortener = urlShortener;
            AuthContext = authContext;
            WebItemSecurity = webItemSecurity;
            SecurityContext = securityContext;
            SettingsManager = settingsManager;
            MobileAppInstallRegistrator = mobileAppInstallRegistrator;
            Configuration = configuration;
            CoreBaseSettings = coreBaseSettings;
            LicenseReader = licenseReader;
            SetupInfo = setupInfo;
            DocumentServiceLicense = documentServiceLicense;
            TenantExtra = tenantExtra;
            ClientFactory = clientFactory;
            _apiSystemHelper = apiSystemHelper;
            _coreSettings = coreSettings;
            _studioNotifyService = studioNotifyService;
            _messageService = messageService;
            _messageTarget = messageTarget;
            _displayUserSettingsHelper = displayUserSettingsHelper;
            _permissionContext = permissionContext;
        }

        [Read("")]
        public Tenant Get()
        {
            return Tenant;
        }

        [Read("users/{userID}")]
        public UserInfo GetUser(Guid userID)
        {
            return UserManager.GetUsers(userID);
        }

        [Read("users/invite/{employeeType}")]
        public object GeInviteLink(EmployeeType employeeType)
        {
            if (!WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, AuthContext.CurrentAccount.ID))
            {
                throw new SecurityException("Method not available");
            }

            return CommonLinkUtility.GetConfirmationUrl(string.Empty, ConfirmType.LinkInvite, (int)employeeType)
                   + $"&emplType={employeeType:d}";
        }

        [Update("getshortenlink")]
        public async Task<object> GetShortenLinkAsync(ShortenLinkModel model)
        {
            try
            {
                return await UrlShortener.Instance.GetShortenLinkAsync(model.Link);
            }
            catch (Exception ex)
            {
                Log.Error("getshortenlink", ex);
                return model.Link;
            }
        }

        [Read("tenantextra")]
        public async Task<object> GetTenantExtraAsync()
        {
            return new
            {
                customMode = CoreBaseSettings.CustomMode,
                opensource = TenantExtra.Opensource,
                enterprise = TenantExtra.Enterprise,
                tariff = TenantExtra.GetCurrentTariff(),
                quota = TenantExtra.GetTenantQuota(),
                notPaid = TenantExtra.IsNotPaid(),
                licenseAccept = SettingsManager.LoadForCurrentUser<TariffSettings>().LicenseAcceptSetting,
                enableTariffPage = //TenantExtra.EnableTarrifSettings - think about hide-settings for opensource
                    (!CoreBaseSettings.Standalone || !string.IsNullOrEmpty(LicenseReader.LicensePath))
                    && string.IsNullOrEmpty(SetupInfo.AmiMetaUrl)
                    && !CoreBaseSettings.CustomMode,
                DocServerUserQuota = await DocumentServiceLicense.GetLicenseQuotaAsync(),
                DocServerLicense = await DocumentServiceLicense.GetLicenseAsync()
            };
        }


        [Read("usedspace")]
        public double GetUsedSpace()
        {
            return Math.Round(
                TenantManager.FindTenantQuotaRows(Tenant.TenantId)
                           .Where(q => !string.IsNullOrEmpty(q.Tag) && new Guid(q.Tag) != Guid.Empty)
                           .Sum(q => q.Counter) / 1024f / 1024f / 1024f, 2);
        }


        [Read("userscount")]
        public long GetUsersCount()
        {
            return CoreBaseSettings.Personal ? 1 : UserManager.GetUserNames(EmployeeStatus.Active).Length;
        }

        [Read("tariff")]
        public Tariff GetTariff()
        {
            return PaymentManager.GetTariff(Tenant.TenantId);
        }

        [Read("quota")]
        public TenantQuota GetQuota()
        {
            return TenantManager.GetTenantQuota(Tenant.TenantId);
        }

        [Read("quota/right")]
        public TenantQuota GetRightQuota()
        {
            var usedSpace = GetUsedSpace();
            var needUsersCount = GetUsersCount();

            return TenantManager.GetTenantQuotas().OrderBy(r => r.Price)
                              .FirstOrDefault(quota =>
                                              quota.ActiveUsers > needUsersCount
                                              && quota.MaxTotalSize > usedSpace
                                              && !quota.Year);
        }


        [Read("path")]
        public object GetFullAbsolutePath(string virtualPath)
        {
            return CommonLinkUtility.GetFullAbsolutePath(virtualPath);
        }

        [Read("thumb")]
        public FileResult GetThumb(string url)
        {
            if (!SecurityContext.IsAuthenticated || Configuration["bookmarking:thumbnail-url"] == null)
            {
                return null;
            }

            url = url.Replace("&amp;", "&");
            url = WebUtility.UrlEncode(url);

            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(string.Format(Configuration["bookmarking:thumbnail-url"], url));

            var httpClient = ClientFactory.CreateClient();
            using var response = httpClient.Send(request);
            using var stream = response.Content.ReadAsStream();
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);

            string type;
            if (response.Headers.TryGetValues("Content-Type", out var values))
            {
                type = values.First();
            }
            else
            {
                type = "image/png";
            }
            return File(bytes, type);
        }

        [Create("present/mark")]
        public void MarkPresentAsReaded()
        {
            try
            {
                var settings = SettingsManager.LoadForCurrentUser<OpensourceGiftSettings>();
                settings.Readed = true;
                SettingsManager.SaveForCurrentUser(settings);
            }
            catch (Exception ex)
            {
                Log.Error("MarkPresentAsReaded", ex);
            }
        }

        [Create("mobile/registration")]
        public void RegisterMobileAppInstallFromBody([FromBody] MobileAppModel model)
        {
            var currentUser = UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            MobileAppInstallRegistrator.RegisterInstall(currentUser.Email, model.Type);
        }

        [Create("mobile/registration")]
        [Consumes("application/x-www-form-urlencoded")]
        public void RegisterMobileAppInstallFromForm([FromForm] MobileAppModel model)
        {
            var currentUser = UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            MobileAppInstallRegistrator.RegisterInstall(currentUser.Email, model.Type);
        }

        [Create("mobile/registration")]
        public void RegisterMobileAppInstall(MobileAppType type)
        {
            var currentUser = UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            MobileAppInstallRegistrator.RegisterInstall(currentUser.Email, type);
        }

        /// <summary>
        /// Updates a portal name with a new one specified in the request.
        /// </summary>
        /// <short>Update a portal name</short>
        /// <param name="alias">New portal name</param>
        /// <returns>Message about renaming a portal</returns>
        ///<visible>false</visible>
        [Update("portalrename")]
        public async Task<object> UpdatePortalName(PortalRenameModel model)
        {
            if (!SetupInfo.IsVisibleSettings(nameof(ManagementType.PortalSecurity)))
            {
                throw new BillingException(Resource.ErrorNotAllowedOption);
            }

            if (CoreBaseSettings.Personal)
            {
                throw new Exception(Resource.ErrorAccessDenied);
            }

            _permissionContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var alias = model.Alias;
            if (string.IsNullOrEmpty(alias)) throw new ArgumentException(nameof(alias));

            var tenant = TenantManager.GetCurrentTenant();
            var user = UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            var localhost = _coreSettings.BaseDomain == "localhost" || tenant.TenantAlias == "localhost";

            var newAlias = alias.ToLowerInvariant();
            var oldAlias = tenant.TenantAlias;
            var oldVirtualRootPath = CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/');

            if (!string.Equals(newAlias, oldAlias, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!string.IsNullOrEmpty(_apiSystemHelper.ApiSystemUrl))
                {
                    await _apiSystemHelper.ValidatePortalNameAsync(newAlias, user.ID);
                }
                else
                {
                    TenantManager.CheckTenantAddress(newAlias.Trim());
                }

                if (!string.IsNullOrEmpty(_apiSystemHelper.ApiCacheUrl))
                {
                    await _apiSystemHelper.AddTenantToCacheAsync(newAlias, user.ID);
                }

                tenant.TenantAlias = alias;
                tenant = TenantManager.SaveTenant(tenant);


                if (!string.IsNullOrEmpty(_apiSystemHelper.ApiCacheUrl))
                {
                    await _apiSystemHelper.RemoveTenantFromCacheAsync(oldAlias, user.ID);
                }

                if (!localhost || string.IsNullOrEmpty(tenant.MappedDomain))
                {
                    _studioNotifyService.PortalRenameNotify(tenant, oldVirtualRootPath);
                }
            }
            else
            {
                return string.Empty;
            }

            return CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.Auth);
        }

        [Create("suspend")]
        public void SendSuspendInstructions()
        {
            _permissionContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var owner = UserManager.GetUsers(Tenant.OwnerId);
            var suspendUrl = CommonLinkUtility.GetConfirmationUrl(owner.Email, ConfirmType.PortalSuspend);
            var continueUrl = CommonLinkUtility.GetConfirmationUrl(owner.Email, ConfirmType.PortalContinue);

            _studioNotifyService.SendMsgPortalDeactivation(Tenant, suspendUrl, continueUrl);

            _messageService.Send(MessageAction.OwnerSentPortalDeactivationInstructions, _messageTarget.Create(owner.ID), owner.DisplayUserName(false, _displayUserSettingsHelper));
        }

        [Create("delete")]
        public void SendDeleteInstructions()
        {
            _permissionContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            var owner = UserManager.GetUsers(Tenant.OwnerId);

            var showAutoRenewText = !CoreBaseSettings.Standalone &&
                            PaymentManager.GetTariffPayments(Tenant.TenantId).Any() &&
                            !TenantExtra.GetTenantQuota().Trial;

            _studioNotifyService.SendMsgPortalDeletion(Tenant, CommonLinkUtility.GetConfirmationUrl(owner.Email, ConfirmType.PortalRemove), showAutoRenewText);

            _messageService.Send(MessageAction.OwnerSentPortalDeleteInstructions, _messageTarget.Create(owner.ID), owner.DisplayUserName(false, _displayUserSettingsHelper));
        }

        [Update("continue")]
        [Authorize(AuthenticationSchemes = "confirm", Roles = "PortalContinue")]
        public void ContinuePortal()
        {
            Tenant.SetStatus(TenantStatus.Active);
            TenantManager.SaveTenant(Tenant);
        }

        [Update("suspend")]
        [Authorize(AuthenticationSchemes = "confirm", Roles = "PortalSuspend")]
        public void SuspendPortal()
        {
            Tenant.SetStatus(TenantStatus.Suspended);
            TenantManager.SaveTenant(Tenant);
            _messageService.Send(MessageAction.PortalDeactivated);
        }

        [Delete("delete")]
        [Authorize(AuthenticationSchemes = "confirm", Roles = "ProfileRemove")]
        public async Task<object> DeletePortal()
        {
            TenantManager.RemoveTenant(Tenant.TenantId);

            if (!string.IsNullOrEmpty(_apiSystemHelper.ApiCacheUrl))
            {
                await _apiSystemHelper.RemoveTenantFromCacheAsync(Tenant.TenantAlias, SecurityContext.CurrentAccount.ID);
            }

            var owner = UserManager.GetUsers(Tenant.OwnerId);
            var redirectLink = SetupInfo.TeamlabSiteRedirect + "/remove-portal-feedback-form.aspx#";
            var parameters = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"firstname\":\"" + owner.FirstName +
                                                                                    "\",\"lastname\":\"" + owner.LastName +
                                                                                    "\",\"alias\":\"" + Tenant.TenantAlias +
                                                                                    "\",\"email\":\"" + owner.Email + "\"}"));

            redirectLink += HttpUtility.UrlEncode(parameters);

            var authed = false;
            try
            {
                if (!SecurityContext.IsAuthenticated)
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                    authed = true;
                }

                _messageService.Send(MessageAction.PortalDeleted);

            }
            finally
            {
                if (authed) SecurityContext.Logout();
            }

            _studioNotifyService.SendMsgPortalDeletionSuccess(owner, redirectLink);

            return redirectLink;
        }
    }
}