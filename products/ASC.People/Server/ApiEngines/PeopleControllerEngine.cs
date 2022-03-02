﻿using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.People.ApiHelpers;

public abstract class PeopleControllerEngine : ApiControllerEngineBase
{
    protected readonly DisplayUserSettingsHelper _displayUserSettingsHelper;
    protected readonly SetupInfo _setupInfo;
    protected readonly UserPhotoManager _userPhotoManager;
    private readonly IHttpClientFactory _httpClientFactory;

    protected PeopleControllerEngine(
        UserManager userManager,
        AuthContext authContext,
        ApiContext apiContext,
        PermissionContext permissionContext,
        SecurityContext securityContext,
        MessageService messageService,
        MessageTarget messageTarget,
        StudioNotifyService studioNotifyService,
        UserPhotoManager userPhotoManager,
        IHttpClientFactory httpClientFactory,
        DisplayUserSettingsHelper displayUserSettingsHelper,
        SetupInfo setupInfo)
        : base(
            userManager,
            authContext,
            apiContext,
            permissionContext,
            securityContext,
            messageService,
            messageTarget,
            studioNotifyService)
    {
        _userPhotoManager = userPhotoManager;
        _httpClientFactory = httpClientFactory;
        _displayUserSettingsHelper = displayUserSettingsHelper;
        _setupInfo = setupInfo;
    }

    protected UserInfo GetUserInfo(string userNameOrId)
    {
        UserInfo user;
        try
        {
            var userId = new Guid(userNameOrId);
            user = _userManager.GetUsers(userId);
        }
        catch (FormatException)
        {
            user = _userManager.GetUserByUserName(userNameOrId);
        }

        if (user == null || user.Id == Constants.LostUser.Id)
        {
            throw new ItemNotFoundException("user not found");
        }

        return user;
    }

    protected void UpdateContacts(IEnumerable<Contact> contacts, UserInfo user)
    {
        _permissionContext.DemandPermissions(new UserSecurityProvider(user.Id), Constants.Action_EditUser);

        if (contacts == null)
        {
            return;
        }

        var values = contacts.Where(r => !string.IsNullOrEmpty(r.Value)).Select(r => $"{r.Type}|{r.Value}");
        user.Contacts = string.Join('|', values);
    }

    protected void UpdatePhotoUrl(string files, UserInfo user)
    {
        if (string.IsNullOrEmpty(files))
        {
            return;
        }

        _permissionContext.DemandPermissions(new UserSecurityProvider(user.Id), Constants.Action_EditUser);

        if (!files.StartsWith("http://") && !files.StartsWith("https://"))
        {
            files = new Uri(_apiContext.HttpContextAccessor.HttpContext.Request.GetDisplayUrl()).GetLeftPart(UriPartial.Authority) + "/" + files.TrimStart('/');
        }
        var request = new HttpRequestMessage();
        request.RequestUri = new Uri(files);

        var httpClient = _httpClientFactory.CreateClient();
        using var response = httpClient.Send(request);
        using var inputStream = response.Content.ReadAsStream();
        using var br = new BinaryReader(inputStream);
        var imageByteArray = br.ReadBytes((int)inputStream.Length);
        _userPhotoManager.SaveOrUpdatePhoto(user.Id, imageByteArray);
    }
}