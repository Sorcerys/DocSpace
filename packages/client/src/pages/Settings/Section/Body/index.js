import React, { useCallback } from "react";
import { useTranslation } from "react-i18next";
import styled, { css } from "styled-components";
import { withRouter } from "react-router";
import Error520 from "client/Error520";
import { inject, observer } from "mobx-react";
import { combineUrl } from "@docspace/common/utils";
import config from "PACKAGE_FILE";
import Submenu from "@docspace/components/submenu";
import CommonSettings from "./CommonSettings";
import AdminSettings from "./AdminSettings";
import { tablet } from "@docspace/components/utils/device";
import { isMobile } from "react-device-detect";

const StyledContainer = styled.div`
  margin-top: -22px;

  @media ${tablet} {
    margin-top: 0px;
  }

  ${isMobile &&
  css`
    margin-top: 0px;
  `}
`;

const SectionBodyContent = ({ isVisitor, isErrorSettings, history }) => {
  const { t } = useTranslation(["FilesSettings", "Common"]);

  const setting = window.location.pathname.endsWith("/settings/common")
    ? "common"
    : "admin";

  const commonSettings = {
    id: "common",
    name: t("Common:Common"),
    content: <CommonSettings t={t} />,
  };

  const adminSettings = {
    id: "admin",
    name: t("Common:AdminSettings"),
    content: <AdminSettings t={t} />,
  };

  const data = [adminSettings, commonSettings];

  const onSelect = useCallback(
    (e) => {
      const { id } = e;

      if (id === setting) return;

      history.push(
        combineUrl(
          window.DocSpaceConfig?.proxy?.url,
          config.homepage,
          `/settings/${id}`
        )
      );
    },
    [setting, history]
  );

  return isErrorSettings ? (
    <Error520 />
  ) : (
    <StyledContainer>
      {isVisitor ? (
        <CommonSettings t={t} showTitle={true} />
      ) : (
        <Submenu
          data={data}
          startSelect={setting === "common" ? commonSettings : adminSettings}
          onSelect={onSelect}
        />
      )}
    </StyledContainer>
  );
};

export default inject(({ auth, settingsStore }) => {
  const { settingsIsLoaded } = settingsStore;

  return {
    isVisitor: auth.userStore.user.isVisitor,
    settingsIsLoaded,
  };
})(withRouter(observer(SectionBodyContent)));
