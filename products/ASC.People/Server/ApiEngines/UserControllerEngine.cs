﻿using Module = ASC.Api.Core.Module;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.People.ApiHelpers;

[Scope]
public class UserControllerEngine : PeopleControllerEngine
{
    private readonly Constants _constants;
    private readonly CookiesManager _cookiesManager;
    private readonly CoreBaseSettings _coreBaseSettings;
    private readonly CustomNamingPeople _customNamingPeople;
    private readonly EmployeeDtoHelper _employeeDtoHelper;
    private readonly EmployeeFullDtoHelper _employeeFullDtoHelper;
    private readonly ILog _logger;
    private readonly PasswordHasher _passwordHasher;
    private readonly QueueWorkerReassign _queueWorkerReassign;
    private readonly QueueWorkerRemove _queueWorkerRemove;
    private readonly Recaptcha _recaptcha;
    private readonly TenantExtra _tenantExtra;
    private readonly TenantStatisticsProvider _tenantStatisticsProvider;
    private readonly TenantUtil _tenantUtil;
    private readonly UserFormatter _userFormatter;
    private readonly UserManagerWrapper _userManagerWrapper;
    private readonly WebItemManager _webItemManager;
    private readonly WebItemSecurity _webItemSecurity;
    private readonly WebItemSecurityCache _webItemSecurityCache;

    public UserControllerEngine(
        UserManager userManager,
        AuthContext authContext,
        ApiContext apiContext,
        PermissionContext permissionContext,
        SecurityContext securityContext,
        MessageService messageService,
        MessageTarget messageTarget,
        StudioNotifyService studioNotifyService,
        Constants constants,
        CookiesManager cookiesManager,
        CoreBaseSettings coreBaseSettings,
        CustomNamingPeople customNamingPeople,
        ILog logger,
        PasswordHasher passwordHasher,
        QueueWorkerReassign queueWorkerReassign,
        QueueWorkerRemove queueWorkerRemove,
        Recaptcha recaptcha,
        TenantExtra tenantExtra,
        TenantStatisticsProvider tenantStatisticsProvider,
        TenantUtil tenantUtil,
        UserFormatter userFormatter,
        UserManagerWrapper userManagerWrapper,
        WebItemManager webItemManager,
        WebItemSecurity webItemSecurity,
        WebItemSecurityCache webItemSecurityCache,
        UserPhotoManager userPhotoManager,
        IHttpClientFactory httpClientFactory,
        DisplayUserSettingsHelper displayUserSettingsHelper,
        SetupInfo setupInfo,
        EmployeeFullDtoHelper employeeFullDtoHelper,
        EmployeeDtoHelper employeeDtoHelper)
        : base(
            userManager,
            authContext,
            apiContext,
            permissionContext,
            securityContext,
            messageService,
            messageTarget,
            studioNotifyService,
            userPhotoManager,
            httpClientFactory,
            displayUserSettingsHelper,
            setupInfo)
    {
        _constants = constants;
        _cookiesManager = cookiesManager;
        _coreBaseSettings = coreBaseSettings;
        _customNamingPeople = customNamingPeople;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _queueWorkerReassign = queueWorkerReassign;
        _queueWorkerRemove = queueWorkerRemove;
        _recaptcha = recaptcha;
        _tenantExtra = tenantExtra;
        _tenantStatisticsProvider = tenantStatisticsProvider;
        _tenantUtil = tenantUtil;
        _userFormatter = userFormatter;
        _userManagerWrapper = userManagerWrapper;
        _webItemManager = webItemManager;
        _webItemSecurity = webItemSecurity;
        _webItemSecurityCache = webItemSecurityCache;
        _employeeDtoHelper = employeeDtoHelper;
        _employeeFullDtoHelper = employeeFullDtoHelper;
    }

    public EmployeeDto AddMember(MemberRequestDto memberModel)
    {
        _apiContext.AuthByClaim();

        _permissionContext.DemandPermissions(Constants.Action_AddRemoveUser);

        memberModel.PasswordHash = (memberModel.PasswordHash ?? "").Trim();
        if (string.IsNullOrEmpty(memberModel.PasswordHash))
        {
            memberModel.Password = (memberModel.Password ?? "").Trim();

            if (string.IsNullOrEmpty(memberModel.Password))
            {
                memberModel.Password = UserManagerWrapper.GeneratePassword();
            }
            else
            {
                _userManagerWrapper.CheckPasswordPolicy(memberModel.Password);
            }
            memberModel.PasswordHash = _passwordHasher.GetClientPassword(memberModel.Password);
        }

        var user = new UserInfo();

        //Validate email
        var address = new MailAddress(memberModel.Email);
        user.Email = address.Address;
        //Set common fields
        user.FirstName = memberModel.Firstname;
        user.LastName = memberModel.Lastname;
        user.Title = memberModel.Title;
        user.Location = memberModel.Location;
        user.Notes = memberModel.Comment;
        user.Sex = "male".Equals(memberModel.Sex, StringComparison.OrdinalIgnoreCase)
                       ? true
                       : ("female".Equals(memberModel.Sex, StringComparison.OrdinalIgnoreCase) ? (bool?)false : null);

        user.BirthDate = memberModel.Birthday != null && memberModel.Birthday != DateTime.MinValue ? _tenantUtil.DateTimeFromUtc(memberModel.Birthday) : null;
        user.WorkFromDate = memberModel.Worksfrom != null && memberModel.Worksfrom != DateTime.MinValue ? _tenantUtil.DateTimeFromUtc(memberModel.Worksfrom) : DateTime.UtcNow.Date;

        UpdateContacts(memberModel.Contacts, user);

        user = _userManagerWrapper.AddUser(user, memberModel.PasswordHash, memberModel.FromInviteLink, true, memberModel.IsVisitor, memberModel.FromInviteLink);

        var messageAction = memberModel.IsVisitor ? MessageAction.GuestCreated : MessageAction.UserCreated;
        _messageService.Send(messageAction, _messageTarget.Create(user.Id), user.DisplayUserName(false, _displayUserSettingsHelper));

        UpdateDepartments(memberModel.Department, user);

        if (memberModel.Files != _userPhotoManager.GetDefaultPhotoAbsoluteWebPath())
        {
            UpdatePhotoUrl(memberModel.Files, user);
        }

        return _employeeFullDtoHelper.GetFull(user);
    }

    public EmployeeDto AddMemberAsActivated(MemberRequestDto memberModel)
    {
        _permissionContext.DemandPermissions(Constants.Action_AddRemoveUser);

        var user = new UserInfo();

        memberModel.PasswordHash = (memberModel.PasswordHash ?? "").Trim();
        if (string.IsNullOrEmpty(memberModel.PasswordHash))
        {
            memberModel.Password = (memberModel.Password ?? "").Trim();

            if (string.IsNullOrEmpty(memberModel.Password))
            {
                memberModel.Password = UserManagerWrapper.GeneratePassword();
            }
            else
            {
                _userManagerWrapper.CheckPasswordPolicy(memberModel.Password);
            }

            memberModel.PasswordHash = _passwordHasher.GetClientPassword(memberModel.Password);
        }

        //Validate email
        var address = new MailAddress(memberModel.Email);
        user.Email = address.Address;
        //Set common fields
        user.FirstName = memberModel.Firstname;
        user.LastName = memberModel.Lastname;
        user.Title = memberModel.Title;
        user.Location = memberModel.Location;
        user.Notes = memberModel.Comment;
        user.Sex = "male".Equals(memberModel.Sex, StringComparison.OrdinalIgnoreCase)
                       ? true
                       : ("female".Equals(memberModel.Sex, StringComparison.OrdinalIgnoreCase) ? (bool?)false : null);

        user.BirthDate = memberModel.Birthday != null ? _tenantUtil.DateTimeFromUtc(memberModel.Birthday) : null;
        user.WorkFromDate = memberModel.Worksfrom != null ? _tenantUtil.DateTimeFromUtc(memberModel.Worksfrom) : DateTime.UtcNow.Date;

        UpdateContacts(memberModel.Contacts, user);

        user = _userManagerWrapper.AddUser(user, memberModel.PasswordHash, false, false, memberModel.IsVisitor);

        user.ActivationStatus = EmployeeActivationStatus.Activated;

        UpdateDepartments(memberModel.Department, user);

        if (memberModel.Files != _userPhotoManager.GetDefaultPhotoAbsoluteWebPath())
        {
            UpdatePhotoUrl(memberModel.Files, user);
        }

        return _employeeFullDtoHelper.GetFull(user);
    }

    public EmployeeDto ChangeUserPassword(Guid userid, MemberRequestDto memberModel)
    {
        _apiContext.AuthByClaim();
        _permissionContext.DemandPermissions(new UserSecurityProvider(userid), Constants.Action_EditUser);

        var user = _userManager.GetUsers(userid);

        if (!_userManager.UserExists(user))
        {
            return null;
        }

        if (_userManager.IsSystemUser(user.Id))
        {
            throw new SecurityException();
        }

        if (!string.IsNullOrEmpty(memberModel.Email))
        {
            var address = new MailAddress(memberModel.Email);
            if (!string.Equals(address.Address, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = address.Address.ToLowerInvariant();
                user.ActivationStatus = EmployeeActivationStatus.Activated;
                _userManager.SaveUserInfo(user);
            }
        }

        memberModel.PasswordHash = (memberModel.PasswordHash ?? "").Trim();
        if (string.IsNullOrEmpty(memberModel.PasswordHash))
        {
            memberModel.Password = (memberModel.Password ?? "").Trim();

            if (!string.IsNullOrEmpty(memberModel.Password))
            {
                memberModel.PasswordHash = _passwordHasher.GetClientPassword(memberModel.Password);
            }
        }

        if (!string.IsNullOrEmpty(memberModel.PasswordHash))
        {
            _securityContext.SetUserPasswordHash(userid, memberModel.PasswordHash);
            _messageService.Send(MessageAction.UserUpdatedPassword);

            _cookiesManager.ResetUserCookie(userid);
            _messageService.Send(MessageAction.CookieSettingsUpdated);
        }

        return _employeeFullDtoHelper.GetFull(GetUserInfo(userid.ToString()));
    }

    public EmployeeDto DeleteMember(string userid)
    {
        _permissionContext.DemandPermissions(Constants.Action_AddRemoveUser);

        var user = GetUserInfo(userid);

        if (_userManager.IsSystemUser(user.Id) || user.IsLDAP())
        {
            throw new SecurityException();
        }

        if (user.Status != EmployeeStatus.Terminated)
        {
            throw new Exception("The user is not suspended");
        }

        CheckReassignProccess(new[] { user.Id });

        var userName = user.DisplayUserName(false, _displayUserSettingsHelper);
        _userPhotoManager.RemovePhoto(user.Id);
        _userManager.DeleteUser(user.Id);
        _queueWorkerRemove.Start(Tenant.Id, user, _securityContext.CurrentAccount.ID, false);

        _messageService.Send(MessageAction.UserDeleted, _messageTarget.Create(user.Id), userName);

        return _employeeFullDtoHelper.GetFull(user);
    }

    public EmployeeDto DeleteProfile()
    {
        _apiContext.AuthByClaim();

        if (_userManager.IsSystemUser(_securityContext.CurrentAccount.ID))
        {
            throw new SecurityException();
        }

        var user = GetUserInfo(_securityContext.CurrentAccount.ID.ToString());

        if (!_userManager.UserExists(user))
        {
            throw new Exception(Resource.ErrorUserNotFound);
        }

        if (user.IsLDAP())
        {
            throw new SecurityException();
        }

        _securityContext.AuthenticateMeWithoutCookie(ASC.Core.Configuration.Constants.CoreSystem);
        user.Status = EmployeeStatus.Terminated;

        _userManager.SaveUserInfo(user);
        var userName = user.DisplayUserName(false, _displayUserSettingsHelper);
        _messageService.Send(MessageAction.UsersUpdatedStatus, _messageTarget.Create(user.Id), userName);

        _cookiesManager.ResetUserCookie(user.Id);
        _messageService.Send(MessageAction.CookieSettingsUpdated);

        if (_coreBaseSettings.Personal)
        {
            _userPhotoManager.RemovePhoto(user.Id);
            _userManager.DeleteUser(user.Id);
            _messageService.Send(MessageAction.UserDeleted, _messageTarget.Create(user.Id), userName);
        }
        else
        {
            //StudioNotifyService.Instance.SendMsgProfileHasDeletedItself(user);
            //StudioNotifyService.SendMsgProfileDeletion(Tenant.TenantId, user);
        }

        return _employeeFullDtoHelper.GetFull(user);
    }

    public IEnumerable<EmployeeDto> GetAdvanced(EmployeeStatus status, string query)
    {
        if (_coreBaseSettings.Personal)
        {
            throw new MethodAccessException("Method not available");
        }
        try
        {
            var list = _userManager.GetUsers(status).AsEnumerable();

            if ("group".Equals(_apiContext.FilterBy, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_apiContext.FilterValue))
            {
                var groupId = new Guid(_apiContext.FilterValue);
                //Filter by group
                list = list.Where(x => _userManager.IsUserInGroup(x.Id, groupId));
                _apiContext.SetDataFiltered();
            }

            list = list.Where(x => x.FirstName != null && x.FirstName.IndexOf(query, StringComparison.OrdinalIgnoreCase) > -1 || (x.LastName != null && x.LastName.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1) ||
                                   (x.UserName != null && x.UserName.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1) || (x.Email != null && x.Email.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1) || (x.ContactsList != null && x.ContactsList.Any(y => y.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1)));

            return list.Select(u => _employeeFullDtoHelper.GetFull(u));
        }
        catch (Exception error)
        {
            _logger.Error(error);
        }

        return null;
    }

    public EmployeeDto GetByEmail(string email)
    {
        if (_coreBaseSettings.Personal && !_userManager.GetUsers(_securityContext.CurrentAccount.ID).IsOwner(Tenant))
        {
            throw new MethodAccessException("Method not available");
        }

        var user = _userManager.GetUserByEmail(email);
        if (user.Id == Constants.LostUser.Id)
        {
            throw new ItemNotFoundException("User not found");
        }

        return _employeeFullDtoHelper.GetFull(user);
    }

    public EmployeeDto GetById(string username)
    {
        if (_coreBaseSettings.Personal)
        {
            throw new MethodAccessException("Method not available");
        }

        var user = _userManager.GetUserByUserName(username);
        if (user.Id == Constants.LostUser.Id)
        {
            if (Guid.TryParse(username, out var userId))
            {
                user = _userManager.GetUsers(userId);
            }
            else
            {
                _logger.Error(string.Format("Account {0} сould not get user by name {1}", _securityContext.CurrentAccount.ID, username));
            }
        }

        if (user.Id == Constants.LostUser.Id)
        {
            throw new ItemNotFoundException("User not found");
        }

        return _employeeFullDtoHelper.GetFull(user);
    }

    public IEnumerable<EmployeeDto> GetByStatus(EmployeeStatus status)
    {
        if (_coreBaseSettings.Personal)
        {
            throw new Exception("Method not available");
        }

        Guid? groupId = null;
        if ("group".Equals(_apiContext.FilterBy, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_apiContext.FilterValue))
        {
            groupId = new Guid(_apiContext.FilterValue);
            _apiContext.SetDataFiltered();
        }

        return GetFullByFilter(status, groupId, null, null, null);
    }

    public IEnumerable<EmployeeDto> GetFullByFilter(EmployeeStatus? employeeStatus, Guid? groupId, EmployeeActivationStatus? activationStatus, EmployeeType? employeeType, bool? isAdministrator)
    {
        var users = GetByFilter(employeeStatus, groupId, activationStatus, employeeType, isAdministrator).AsEnumerable();

        return users.Select(u => _employeeFullDtoHelper.GetFull(u));
    }

    public Module GetModule()
    {
        var product = new PeopleProduct();
        product.Init();

        return new Module(product);
    }

    public IEnumerable<EmployeeDto> GetSearch(string query)
    {
        if (_coreBaseSettings.Personal)
        {
            throw new MethodAccessException("Method not available");
        }

        try
        {
            var groupId = Guid.Empty;
            if ("group".Equals(_apiContext.FilterBy, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_apiContext.FilterValue))
            {
                groupId = new Guid(_apiContext.FilterValue);
            }

            var users = _userManager.Search(query, EmployeeStatus.Active, groupId);

            return users.Select(u => _employeeFullDtoHelper.GetFull(u));
        }
        catch (Exception error)
        {
            _logger.Error(error);
        }

        return null;
    }

    public IEnumerable<EmployeeDto> GetSimpleByFilter(EmployeeStatus? employeeStatus, Guid? groupId, EmployeeActivationStatus? activationStatus, EmployeeType? employeeType, bool? isAdministrator)
    {
        var users = GetByFilter(employeeStatus, groupId, activationStatus, employeeType, isAdministrator);

        return users.Select(u => _employeeDtoHelper.Get(u));
    }

    public Task<string> RegisterUserOnPersonalAsync(RegisterPersonalUserRequestDto model, HttpRequest request)
    {
        if (!_coreBaseSettings.Personal)
        {
            throw new MethodAccessException("Method is only available on personal.onlyoffice.com");
        }

        return InternalRegisterUserOnPersonalAsync(model, request);
    }

    public IEnumerable<EmployeeDto> RemoveUsers(UpdateMembersRequestDto model)
    {
        _permissionContext.DemandPermissions(Constants.Action_AddRemoveUser);

        CheckReassignProccess(model.UserIds);

        var users = model.UserIds.Select(userId => _userManager.GetUsers(userId))
            .Where(u => !_userManager.IsSystemUser(u.Id) && !u.IsLDAP())
            .ToList();

        var userNames = users.Select(x => x.DisplayUserName(false, _displayUserSettingsHelper)).ToList();

        foreach (var user in users)
        {
            if (user.Status != EmployeeStatus.Terminated)
            {
                continue;
            }

            _userPhotoManager.RemovePhoto(user.Id);
            _userManager.DeleteUser(user.Id);
            _queueWorkerRemove.Start(Tenant.Id, user, _securityContext.CurrentAccount.ID, false);
        }

        _messageService.Send(MessageAction.UsersDeleted, _messageTarget.Create(users.Select(x => x.Id)), userNames);

        return users.Select(u => _employeeFullDtoHelper.GetFull(u));
    }

    public IEnumerable<EmployeeDto> ResendUserInvites(UpdateMembersRequestDto model)
    {
        var users = model.UserIds
            .Where(userId => !_userManager.IsSystemUser(userId))
            .Select(userId => _userManager.GetUsers(userId))
            .ToList();

        foreach (var user in users)
        {
            if (user.IsActive)
            {
                continue;
            }

            var viewer = _userManager.GetUsers(_securityContext.CurrentAccount.ID);

            if (viewer == null)
            {
                throw new Exception(Resource.ErrorAccessDenied);
            }

            if (viewer.IsAdmin(_userManager) || viewer.Id == user.Id)
            {
                if (user.ActivationStatus == EmployeeActivationStatus.Activated)
                {
                    user.ActivationStatus = EmployeeActivationStatus.NotActivated;
                }
                if (user.ActivationStatus == (EmployeeActivationStatus.AutoGenerated | EmployeeActivationStatus.Activated))
                {
                    user.ActivationStatus = EmployeeActivationStatus.AutoGenerated;
                }

                _userManager.SaveUserInfo(user);
            }

            if (user.ActivationStatus == EmployeeActivationStatus.Pending)
            {
                if (user.IsVisitor(_userManager))
                {
                    _studioNotifyService.GuestInfoActivation(user);
                }
                else
                {
                    _studioNotifyService.UserInfoActivation(user);
                }
            }
            else
            {
                _studioNotifyService.SendEmailActivationInstructions(user, user.Email);
            }
        }

        _messageService.Send(MessageAction.UsersSentActivationInstructions, _messageTarget.Create(users.Select(x => x.Id)), users.Select(x => x.DisplayUserName(false, _displayUserSettingsHelper)));

        return users.Select(u => _employeeFullDtoHelper.GetFull(u));
    }

    public EmployeeDto Self()
    {
        var user = _userManager.GetUser(_securityContext.CurrentAccount.ID, EmployeeFullDtoHelper.GetExpression(_apiContext));

        return _employeeFullDtoHelper.GetFull(user);
    }

    public object SendEmailChangeInstructions(UpdateMemberRequestDto model)
    {
        Guid.TryParse(model.UserId, out var userid);

        if (userid == Guid.Empty)
        {
            throw new ArgumentNullException("userid");
        }

        var email = (model.Email ?? "").Trim();

        if (string.IsNullOrEmpty(email))
        {
            throw new Exception(Resource.ErrorEmailEmpty);
        }

        if (!email.TestEmailRegex())
        {
            throw new Exception(Resource.ErrorNotCorrectEmail);
        }

        var viewer = _userManager.GetUsers(_securityContext.CurrentAccount.ID);
        var user = _userManager.GetUsers(userid);

        if (user == null)
        {
            throw new Exception(Resource.ErrorUserNotFound);
        }

        if (viewer == null || (user.IsOwner(Tenant) && viewer.Id != user.Id))
        {
            throw new Exception(Resource.ErrorAccessDenied);
        }

        var existentUser = _userManager.GetUserByEmail(email);

        if (existentUser.Id != Constants.LostUser.Id)
        {
            throw new Exception(_customNamingPeople.Substitute<Resource>("ErrorEmailAlreadyExists"));
        }

        if (!viewer.IsAdmin(_userManager))
        {
            _studioNotifyService.SendEmailChangeInstructions(user, email);
        }
        else
        {
            if (email == user.Email)
            {
                throw new Exception(Resource.ErrorEmailsAreTheSame);
            }

            user.Email = email;
            user.ActivationStatus = EmployeeActivationStatus.NotActivated;
            _userManager.SaveUserInfo(user);
            _studioNotifyService.SendEmailActivationInstructions(user, email);
        }

        _messageService.Send(MessageAction.UserSentEmailChangeInstructions, user.DisplayUserName(false, _displayUserSettingsHelper));

        return string.Format(Resource.MessageEmailChangeInstuctionsSentOnEmail, email);
    }

    public object SendUserPassword(MemberRequestDto memberModel)
    {
        string error = _userManagerWrapper.SendUserPassword(memberModel.Email);
        if (!string.IsNullOrEmpty(error))
        {
            _logger.ErrorFormat("Password recovery ({0}): {1}", memberModel.Email, error);
        }

        return string.Format(Resource.MessageYourPasswordSendedToEmail, memberModel.Email);
    }

    public IEnumerable<EmployeeDto> UpdateEmployeeActivationStatus(EmployeeActivationStatus activationstatus, UpdateMembersRequestDto model)
    {
        _apiContext.AuthByClaim();

        var retuls = new List<EmployeeDto>();
        foreach (var id in model.UserIds.Where(userId => !_userManager.IsSystemUser(userId)))
        {
            _permissionContext.DemandPermissions(new UserSecurityProvider(id), Constants.Action_EditUser);
            var u = _userManager.GetUsers(id);
            if (u.Id == Constants.LostUser.Id || u.IsLDAP())
            {
                continue;
            }

            u.ActivationStatus = activationstatus;
            _userManager.SaveUserInfo(u);
            retuls.Add(_employeeFullDtoHelper.GetFull(u));
        }

        return retuls;
    }

    public EmployeeDto UpdateMember(string userid, UpdateMemberRequestDto memberModel)
    {
        var user = GetUserInfo(userid);

        if (_userManager.IsSystemUser(user.Id))
        {
            throw new SecurityException();
        }

        _permissionContext.DemandPermissions(new UserSecurityProvider(user.Id), Constants.Action_EditUser);
        var self = _securityContext.CurrentAccount.ID.Equals(user.Id);
        var resetDate = new DateTime(1900, 01, 01);

        //Update it

        var isLdap = user.IsLDAP();
        var isSso = user.IsSSO();
        var isAdmin = _webItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, _securityContext.CurrentAccount.ID);

        if (!isLdap && !isSso)
        {
            //Set common fields

            user.FirstName = memberModel.Firstname ?? user.FirstName;
            user.LastName = memberModel.Lastname ?? user.LastName;
            user.Location = memberModel.Location ?? user.Location;

            if (isAdmin)
            {
                user.Title = memberModel.Title ?? user.Title;
            }
        }

        if (!_userFormatter.IsValidUserName(user.FirstName, user.LastName))
        {
            throw new Exception(Resource.ErrorIncorrectUserName);
        }

        user.Notes = memberModel.Comment ?? user.Notes;
        user.Sex = ("male".Equals(memberModel.Sex, StringComparison.OrdinalIgnoreCase)
            ? true
            : ("female".Equals(memberModel.Sex, StringComparison.OrdinalIgnoreCase) ? (bool?)false : null)) ?? user.Sex;

        user.BirthDate = memberModel.Birthday != null ? _tenantUtil.DateTimeFromUtc(memberModel.Birthday) : user.BirthDate;

        if (user.BirthDate == resetDate)
        {
            user.BirthDate = null;
        }

        user.WorkFromDate = memberModel.Worksfrom != null ? _tenantUtil.DateTimeFromUtc(memberModel.Worksfrom) : user.WorkFromDate;

        if (user.WorkFromDate == resetDate)
        {
            user.WorkFromDate = null;
        }

        //Update contacts
        UpdateContacts(memberModel.Contacts, user);
        UpdateDepartments(memberModel.Department, user);

        if (memberModel.Files != _userPhotoManager.GetPhotoAbsoluteWebPath(user.Id))
        {
            UpdatePhotoUrl(memberModel.Files, user);
        }
        if (memberModel.Disable.HasValue)
        {
            user.Status = memberModel.Disable.Value ? EmployeeStatus.Terminated : EmployeeStatus.Active;
            user.TerminatedDate = memberModel.Disable.Value ? DateTime.UtcNow : null;
        }
        if (self && !isAdmin)
        {
            _studioNotifyService.SendMsgToAdminAboutProfileUpdated();
        }

        // change user type
        var canBeGuestFlag = !user.IsOwner(Tenant) && !user.IsAdmin(_userManager) && user.GetListAdminModules(_webItemSecurity).Count == 0 && !user.IsMe(_authContext);

        if (memberModel.IsVisitor && !user.IsVisitor(_userManager) && canBeGuestFlag)
        {
            _userManager.AddUserIntoGroup(user.Id, Constants.GroupVisitor.ID);
            _webItemSecurityCache.ClearCache(Tenant.Id);
        }

        if (!self && !memberModel.IsVisitor && user.IsVisitor(_userManager))
        {
            var usersQuota = _tenantExtra.GetTenantQuota().ActiveUsers;
            if (_tenantStatisticsProvider.GetUsersCount() < usersQuota)
            {
                _userManager.RemoveUserFromGroup(user.Id, Constants.GroupVisitor.ID);
                _webItemSecurityCache.ClearCache(Tenant.Id);
            }
            else
            {
                throw new TenantQuotaException(string.Format("Exceeds the maximum active users ({0})", usersQuota));
            }
        }

        _userManager.SaveUserInfo(user);
        _messageService.Send(MessageAction.UserUpdated, _messageTarget.Create(user.Id), user.DisplayUserName(false, _displayUserSettingsHelper));

        if (memberModel.Disable.HasValue && memberModel.Disable.Value)
        {
            _cookiesManager.ResetUserCookie(user.Id);
            _messageService.Send(MessageAction.CookieSettingsUpdated);
        }

        return _employeeFullDtoHelper.GetFull(user);
    }

    public EmployeeDto UpdateMemberCulture(string userid, UpdateMemberRequestDto memberModel)
    {
        var user = GetUserInfo(userid);

        if (_userManager.IsSystemUser(user.Id))
        {
            throw new SecurityException();
        }

        _permissionContext.DemandPermissions(new UserSecurityProvider(user.Id), Constants.Action_EditUser);

        var curLng = user.CultureName;

        if (_setupInfo.EnabledCultures.Find(c => string.Equals(c.Name, memberModel.CultureName, StringComparison.InvariantCultureIgnoreCase)) != null)
        {
            if (curLng != memberModel.CultureName)
            {
                user.CultureName = memberModel.CultureName;

                try
                {
                    _userManager.SaveUserInfo(user);
                }
                catch
                {
                    user.CultureName = curLng;
                    throw;
                }

                _messageService.Send(MessageAction.UserUpdatedLanguage, _messageTarget.Create(user.Id), user.DisplayUserName(false, _displayUserSettingsHelper));

            }
        }

        return _employeeFullDtoHelper.GetFull(user);
    }

    public IEnumerable<EmployeeDto> UpdateUserStatus(EmployeeStatus status, UpdateMembersRequestDto model)
    {
        _permissionContext.DemandPermissions(Constants.Action_EditUser);

        var users = model.UserIds.Select(userId => _userManager.GetUsers(userId))
            .Where(u => !_userManager.IsSystemUser(u.Id) && !u.IsLDAP())
            .ToList();

        foreach (var user in users)
        {
            if (user.IsOwner(Tenant) || user.IsMe(_authContext))
            {
                continue;
            }

            switch (status)
            {
                case EmployeeStatus.Active:
                    if (user.Status == EmployeeStatus.Terminated)
                    {
                        if (_tenantStatisticsProvider.GetUsersCount() < _tenantExtra.GetTenantQuota().ActiveUsers || user.IsVisitor(_userManager))
                        {
                            user.Status = EmployeeStatus.Active;
                            _userManager.SaveUserInfo(user);
                        }
                    }
                    break;
                case EmployeeStatus.Terminated:
                    user.Status = EmployeeStatus.Terminated;
                    _userManager.SaveUserInfo(user);

                    _cookiesManager.ResetUserCookie(user.Id);
                    _messageService.Send(MessageAction.CookieSettingsUpdated);
                    break;
            }
        }

        _messageService.Send(MessageAction.UsersUpdatedStatus, _messageTarget.Create(users.Select(x => x.Id)), users.Select(x => x.DisplayUserName(false, _displayUserSettingsHelper)));

        return users.Select(u => _employeeFullDtoHelper.GetFull(u));
    }

    public IEnumerable<EmployeeDto> UpdateUserType(EmployeeType type, UpdateMembersRequestDto model)
    {
        var users = model.UserIds
            .Where(userId => !_userManager.IsSystemUser(userId))
            .Select(userId => _userManager.GetUsers(userId))
            .ToList();

        foreach (var user in users)
        {
            if (user.IsOwner(Tenant) || user.IsAdmin(_userManager)
                || user.IsMe(_authContext) || user.GetListAdminModules(_webItemSecurity).Count > 0)
            {
                continue;
            }

            switch (type)
            {
                case EmployeeType.User:
                    if (user.IsVisitor(_userManager))
                    {
                        if (_tenantStatisticsProvider.GetUsersCount() < _tenantExtra.GetTenantQuota().ActiveUsers)
                        {
                            _userManager.RemoveUserFromGroup(user.Id, Constants.GroupVisitor.ID);
                            _webItemSecurityCache.ClearCache(Tenant.Id);
                        }
                    }
                    break;
                case EmployeeType.Visitor:
                    if (_coreBaseSettings.Standalone || _tenantStatisticsProvider.GetVisitorsCount() < _tenantExtra.GetTenantQuota().ActiveUsers * _constants.CoefficientOfVisitors)
                    {
                        _userManager.AddUserIntoGroup(user.Id, Constants.GroupVisitor.ID);
                        _webItemSecurityCache.ClearCache(Tenant.Id);
                    }
                    break;
            }
        }

        _messageService.Send(MessageAction.UsersUpdatedType, _messageTarget.Create(users.Select(x => x.Id)), users.Select(x => x.DisplayUserName(false, _displayUserSettingsHelper)));

        return users.Select(u => _employeeFullDtoHelper.GetFull(u));
    }

    private void CheckReassignProccess(IEnumerable<Guid> userIds)
    {
        foreach (var userId in userIds)
        {
            var reassignStatus = _queueWorkerReassign.GetProgressItemStatus(Tenant.Id, userId);
            if (reassignStatus == null || reassignStatus.IsCompleted)
            {
                continue;
            }

            var userName = _userManager.GetUsers(userId).DisplayUserName(_displayUserSettingsHelper);

            throw new Exception(string.Format(Resource.ReassignDataRemoveUserError, userName));
        }
    }

    private IQueryable<UserInfo> GetByFilter(EmployeeStatus? employeeStatus, Guid? groupId, EmployeeActivationStatus? activationStatus, EmployeeType? employeeType, bool? isAdministrator)
    {
        if (_coreBaseSettings.Personal)
        {
            throw new MethodAccessException("Method not available");
        }

        var isAdmin = _userManager.GetUsers(_securityContext.CurrentAccount.ID).IsAdmin(_userManager) ||
                      _webItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, _securityContext.CurrentAccount.ID);

        var includeGroups = new List<List<Guid>>();
        if (groupId.HasValue)
        {
            includeGroups.Add(new List<Guid> { groupId.Value });
        }

        var excludeGroups = new List<Guid>();

        if (employeeType != null)
        {
            switch (employeeType)
            {
                case EmployeeType.User:
                    excludeGroups.Add(Constants.GroupVisitor.ID);
                    break;
                case EmployeeType.Visitor:
                    includeGroups.Add(new List<Guid> { Constants.GroupVisitor.ID });
                    break;
            }
        }

        if (isAdministrator.HasValue && isAdministrator.Value)
        {
            var adminGroups = new List<Guid>
            {
                    Constants.GroupAdmin.ID
            };
            var products = _webItemManager.GetItemsAll().Where(i => i is IProduct || i.ID == WebItemManager.MailProductID);
            adminGroups.AddRange(products.Select(r => r.ID));

            includeGroups.Add(adminGroups);
        }

        var users = _userManager.GetUsers(isAdmin, employeeStatus, includeGroups, excludeGroups, activationStatus, _apiContext.FilterValue, _apiContext.SortBy, !_apiContext.SortDescending, _apiContext.Count, _apiContext.StartIndex, out var total, out var count);

        _apiContext.SetTotalCount(total).SetCount(count);

        return users;
    }

    private async Task<string> InternalRegisterUserOnPersonalAsync(RegisterPersonalUserRequestDto model, HttpRequest request)
    {
        try
        {
            if (_coreBaseSettings.CustomMode) model.Lang = "ru-RU";

            var cultureInfo = _setupInfo.GetPersonalCulture(model.Lang).Value;

            if (cultureInfo != null)
            {
                Thread.CurrentThread.CurrentUICulture = cultureInfo;
            }

            model.Email.ThrowIfNull(new ArgumentException(Resource.ErrorEmailEmpty, "email"));

            if (!model.Email.TestEmailRegex()) throw new ArgumentException(Resource.ErrorNotCorrectEmail, "email");

            if (!SetupInfo.IsSecretEmail(model.Email)
                && !string.IsNullOrEmpty(_setupInfo.RecaptchaPublicKey) && !string.IsNullOrEmpty(_setupInfo.RecaptchaPrivateKey))
            {
                var ip = request.Headers["X-Forwarded-For"].ToString() ?? request.GetUserHostAddress();

                if (string.IsNullOrEmpty(model.RecaptchaResponse)
                    || !await _recaptcha.ValidateRecaptchaAsync(model.RecaptchaResponse, ip))
                {
                    throw new RecaptchaException(Resource.RecaptchaInvalid);
                }
            }

            var newUserInfo = _userManager.GetUserByEmail(model.Email);

            if (_userManager.UserExists(newUserInfo.Id))
            {
                if (!SetupInfo.IsSecretEmail(model.Email) || _securityContext.IsAuthenticated)
                {
                    _studioNotifyService.SendAlreadyExist(model.Email);
                    return string.Empty;
                }

                try
                {
                    _securityContext.AuthenticateMe(Core.Configuration.Constants.CoreSystem);
                    _userManager.DeleteUser(newUserInfo.Id);
                }
                finally
                {
                    _securityContext.Logout();
                }
            }
            if (!model.Spam)
            {
                try
                {
                    //TODO
                    //const string _databaseID = "com";
                    //using (var db = DbManager.FromHttpContext(_databaseID))
                    //{
                    //    db.ExecuteNonQuery(new SqlInsert("template_unsubscribe", false)
                    //                           .InColumnValue("email", email.ToLowerInvariant())
                    //                           .InColumnValue("reason", "personal")
                    //        );
                    //    Log.Debug(String.Format("Write to template_unsubscribe {0}", email.ToLowerInvariant()));
                    //}
                }
                catch (Exception ex)
                {
                    _logger.Debug($"ERROR write to template_unsubscribe {ex.Message}, email:{model.Email.ToLowerInvariant()}");
                }
            }

            _studioNotifyService.SendInvitePersonal(model.Email);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return string.Empty;
    }

    private void UpdateDepartments(IEnumerable<Guid> department, UserInfo user)
    {
        if (!_permissionContext.CheckPermissions(Constants.Action_EditGroups))
        {
            return;
        }

        if (department == null)
        {
            return;
        }

        var groups = _userManager.GetUserGroups(user.Id);
        var managerGroups = new List<Guid>();
        foreach (var groupInfo in groups)
        {
            _userManager.RemoveUserFromGroup(user.Id, groupInfo.ID);
            var managerId = _userManager.GetDepartmentManager(groupInfo.ID);
            if (managerId == user.Id)
            {
                managerGroups.Add(groupInfo.ID);
                _userManager.SetDepartmentManager(groupInfo.ID, Guid.Empty);
            }
        }
        foreach (var guid in department)
        {
            var userDepartment = _userManager.GetGroupInfo(guid);
            if (userDepartment != Constants.LostGroupInfo)
            {
                _userManager.AddUserIntoGroup(user.Id, guid);
                if (managerGroups.Contains(guid))
                {
                    _userManager.SetDepartmentManager(guid, user.Id);
                }
            }
        }
    }
}