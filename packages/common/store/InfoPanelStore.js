import { makeAutoObservable } from "mobx";

import { getUserRole } from "@docspace/client/src/helpers/people-helpers";
import { getUserById } from "@docspace/common/api/people";
import { combineUrl } from "@docspace/common/utils";
import { AppServerConfig } from "@docspace/common/constants";
import config from "PACKAGE_FILE";
import Filter from "../api/people/filter";

class InfoPanelStore {
  isVisible = false;

  selection = null;
  selectionParentRoom = null;

  roomsView = "members";
  fileView = "history";

  authStore = null;
  settingsStore = null;
  peopleStore = null;
  filesStore = null;
  selectedFolderStore = null;
  treeFoldersStore = null;

  constructor() {
    makeAutoObservable(this);
  }

  // Setters

  setIsVisible = (bool) => (this.isVisible = bool);

  setSelection = (selection) => (this.selection = selection);
  setSelectionParentRoom = (obj) => (this.selectionParentRoom = obj);

  setView = (view) => {
    this.roomsView = view;
    this.fileView = view === "members" ? "history" : view;
  };

  // Selection helpers //

  getSelectedItems = () => {
    const { selection: filesStoreSelection } = this.filesStore;
    const {
      selection: peopleStoreSelection,
      bufferSelection: peopleStoreBufferSelection,
    } = this.peopleStore.selectionStore;

    return this.getIsAccounts()
      ? peopleStoreSelection.length
        ? [...peopleStoreSelection]
        : peopleStoreBufferSelection
        ? [peopleStoreBufferSelection]
        : []
      : filesStoreSelection?.length > 0
      ? [...filesStoreSelection]
      : [];
  };

  getSelectedFolder = () => {
    const selectedFolderStore = { ...this.selectedFolderStore };
    return {
      selectedFolderStore,
      isFolder: true,
      isRoom: !!this.selectedFolderStore.roomType,
    };
  };

  calculateSelection = (
    props = { selectedItems: [], selectedFolder: null }
  ) => {
    const selectedItems = props.selectedItems.length
      ? props.selectedItems
      : this.getSelectedItems();

    const selectedFolder = props.selectedFolder
      ? props.selectedFolder
      : this.getSelectedFolder();

    return selectedItems.length === 0
      ? this.normalizeSelection({
          ...selectedFolder,
          isSelectedFolder: true,
          isSelectedItem: false,
        })
      : selectedItems.length === 1
      ? this.normalizeSelection({
          ...selectedItems[0],
          isSelectedFolder: false,
          isSelectedItem: true,
        })
      : [...Array(selectedItems.length).keys()];
  };

  normalizeSelection = (selection) => {
    const isContextMenuSelection = selection.isContextMenuSelection;
    return {
      ...selection,
      isRoom: selection.isRoom || !!selection.roomType,
      icon: this.getInfoPanelItemIcon(selection, 32),
      isContextMenuSelection: false,
      wasContextMenuSelection: !!isContextMenuSelection,
    };
  };

  reloadSelection = () => {
    this.setSelection(this.calculateSelection());
  };

  // Icon helpers //

  getInfoPanelItemIcon = (item, size) => {
    return item.isRoom || !!item.roomType
      ? item.logo && item.logo.medium
        ? item.logo.medium
        : item.icon
        ? item.icon
        : this.settingsStore.getIcon(size, null, null, null, item.roomType)
      : item.isFolder
      ? this.settingsStore.getFolderIcon(item.providerKey, size)
      : this.settingsStore.getIcon(size, item.fileExst || ".file");
  };

  // User link actions //

  openUser = async (userId, history) => {
    if (userId === this.authStore.userStore.user.id) {
      this.openSelfProfile(history);
      return;
    }

    const fetchedUser = await this.fetchUser(userId);
    this.openAccountsWithSelectedUser(fetchedUser, history);
  };

  openSelfProfile = (history) => {
    const path = [
      AppServerConfig.proxyURL,
      config.homepage,
      "/accounts",
      "/view/@self",
    ];
    this.selectedFolderStore.setSelectedFolder(null);
    this.treeFoldersStore.setSelectedNode(["accounts", "filter"]);
    history.push(combineUrl(...path));
  };

  openAccountsWithSelectedUser = async (user, history) => {
    const { getUsersList } = this.peopleStore.usersStore;
    const { selectUser } = this.peopleStore.selectionStore;

    const path = [AppServerConfig.proxyURL, config.homepage, "/accounts"];

    const newFilter = Filter.getDefault();
    newFilter.page = 0;
    newFilter.search = user.email;
    await getUsersList(newFilter);
    path.push(`filter?${newFilter.toUrlParams()}`);

    this.selectedFolderStore.setSelectedFolder(null);
    this.treeFoldersStore.setSelectedNode(["accounts"]);
    history.push(combineUrl(...path));

    selectUser(user);
  };

  fetchUser = async (userId) => {
    const {
      getStatusType,
      getUserContextOptions,
    } = this.peopleStore.usersStore;

    const fetchedUser = await getUserById(userId);
    fetchedUser.role = getUserRole(fetchedUser);
    fetchedUser.statusType = getStatusType(fetchedUser);
    fetchedUser.options = getUserContextOptions(
      false,
      fetchedUser.isOwner,
      fetchedUser.statusType,
      fetchedUser.status
    );

    return fetchedUser;
  };

  // Routing helpers //

  getCanDisplay = () => {
    const pathname = window.location.pathname.toLowerCase();
    const isFiles = this.getIsFiles(pathname);
    const isRooms = this.getIsRooms(pathname);
    const isAccounts = this.getIsAccounts(pathname);
    const isGallery = this.getIsGallery(pathname);
    return isRooms || isFiles || isGallery || isAccounts;
  };

  getIsFiles = (givenPathName) => {
    const pathname = givenPathName || window.location.pathname.toLowerCase();
    return (
      pathname.indexOf("files") !== -1 || pathname.indexOf("personal") !== -1
    );
  };

  getIsRooms = (givenPathName) => {
    const pathname = givenPathName || window.location.pathname.toLowerCase();
    return (
      pathname.indexOf("rooms") !== -1 && !(pathname.indexOf("personal") !== -1)
    );
  };

  getIsAccounts = (givenPathName) => {
    const pathname = givenPathName || window.location.pathname.toLowerCase();
    return (
      pathname.indexOf("accounts") !== -1 && !(pathname.indexOf("view") !== -1)
    );
  };

  getIsGallery = (givenPathName) => {
    const pathname = givenPathName || window.location.pathname.toLowerCase();
    return pathname.indexOf("form-gallery") !== -1;
  };
}

export default InfoPanelStore;
