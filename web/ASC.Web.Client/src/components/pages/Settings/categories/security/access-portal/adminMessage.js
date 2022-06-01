import React, { useState, useEffect } from "react";
import styled from "styled-components";
import { withRouter } from "react-router";
import { withTranslation } from "react-i18next";
import { inject, observer } from "mobx-react";
import RadioButtonGroup from "@appserver/components/radio-button-group";
import Text from "@appserver/components/text";
import Link from "@appserver/components/link";
import toastr from "@appserver/components/toast/toastr";
import { getLanguage } from "@appserver/common/utils";
import { LearnMoreWrapper } from "../StyledSecurity";
import { size } from "@appserver/components/utils/device";
import { saveToSessionStorage, getFromSessionStorage } from "../../../utils";
import SaveCancelButtons from "@appserver/components/save-cancel-buttons";
import isEqual from "lodash/isEqual";

const MainContainer = styled.div`
  width: 100%;

  .page-subtitle {
    margin-bottom: 10px;
  }

  .box {
    margin-bottom: 24px;
  }
`;

const AdminMessage = (props) => {
  const {
    t,
    history,
    enableAdmMess,
    setMessageSettings,
    initSettings,
    isInit,
  } = props;
  const [type, setType] = useState("");
  const [showReminder, setShowReminder] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const getSettings = () => {
    const currentSettings = getFromSessionStorage(
      "currentAdminMessageSettings"
    );

    const enable = enableAdmMess ? "enable" : "disabled";

    saveToSessionStorage("defaultAdminMessageSettings", enable);

    if (currentSettings) {
      setType(currentSettings);
    } else {
      setType(enable);
    }
  };

  useEffect(() => {
    checkWidth();

    if (!isInit) initSettings().then(() => setIsLoading(true));
    else setIsLoading(true);

    window.addEventListener("resize", checkWidth);
    return () => window.removeEventListener("resize", checkWidth);
  }, []);

  useEffect(() => {
    if (!isInit) return;
    getSettings();
  }, [isLoading]);

  useEffect(() => {
    if (!isLoading) return;

    const defaultSettings = getFromSessionStorage(
      "defaultAdminMessageSettings"
    );
    saveToSessionStorage("currentAdminMessageSettings", type);

    if (isEqual(defaultSettings, type)) {
      console.log("===");
      setShowReminder(false);
    } else {
      setShowReminder(true);
    }
  }, [type]);

  const checkWidth = () => {
    window.innerWidth > size.smallTablet &&
      history.location.pathname.includes("admin-message") &&
      history.push("/settings/security/access-portal");
  };

  const onSelectType = (e) => {
    if (type !== e.target.value) {
      setType(e.target.value);
    }
  };

  const onSaveClick = () => {
    setMessageSettings(type);
    toastr.success(t("SuccessfullySaveSettingsMessage"));
    saveToSessionStorage("defaultAdminMessageSettings", type);
    setShowReminder(false);
  };

  const onCancelClick = () => {
    const defaultSettings = getFromSessionStorage(
      "defaultAdminMessageSettings"
    );
    setType(defaultSettings);
    setShowReminder(false);
  };

  const lng = getLanguage(localStorage.getItem("language") || "en");

  return (
    <MainContainer>
      <LearnMoreWrapper>
        <Text className="page-subtitle">{t("AdminsMessageHelper")}</Text>
        <Link
          color="#316DAA"
          target="_blank"
          isHovered
          href={`https://helpcenter.onlyoffice.com/${lng}/administration/configuration.aspx#ChangingSecuritySettings_block`}
        >
          {t("Common:LearnMore")}
        </Link>
      </LearnMoreWrapper>

      <RadioButtonGroup
        className="box"
        fontSize="13px"
        fontWeight="400"
        name="group"
        orientation="vertical"
        spacing="8px"
        options={[
          {
            label: t("Disabled"),
            value: "disabled",
          },
          {
            label: t("Common:Enable"),
            value: "enable",
          },
        ]}
        selected={type}
        onClick={onSelectType}
      />

      <SaveCancelButtons
        className="save-cancel-buttons"
        onSaveClick={onSaveClick}
        onCancelClick={onCancelClick}
        showReminder={showReminder}
        reminderTest={t("YouHaveUnsavedChanges")}
        saveButtonLabel={t("Common:SaveButton")}
        cancelButtonLabel={t("Common:CancelButton")}
        displaySettings={true}
        hasScroll={false}
      />
    </MainContainer>
  );
};

export default inject(({ auth, setup }) => {
  const { enableAdmMess, setMessageSettings } = auth.settingsStore;
  const { initSettings, isInit } = setup;

  return {
    enableAdmMess,
    setMessageSettings,
    initSettings,
    isInit,
  };
})(withTranslation(["Settings", "Common"])(withRouter(observer(AdminMessage))));
