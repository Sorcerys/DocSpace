import React from "react";
import { withTranslation } from "react-i18next";
import Button from "@appserver/components/button";
import SelectelSettings from "../../consumer-storage-settings/SelectelSettings";

class SelectelStorage extends React.Component {
  constructor(props) {
    super(props);
    const { availableStorage, selectedId } = this.props;

    const formSettings = {};
    this.namesArray = SelectelSettings.formNames();
    this.namesArray.forEach((elem) => (formSettings[elem] = ""));

    this.state = {
      formSettings,
      formErrors: {},
    };

    this.isDisabled =
      availableStorage[selectedId] && !availableStorage[selectedId].isSet;
  }

  onChange = (event) => {
    const { formSettings } = this.state;
    const { target } = event;
    const value = target.value;
    const name = target.name;

    this.setState({ formSettings: { ...formSettings, [name]: value } });
  };

  onMakeCopy = () => {
    const { formSettings } = this.state;

    const { onMakeCopyIntoStorage, isInvalidForm } = this.props;

    const isInvalid = isInvalidForm(formSettings);

    const hasError = isInvalid[0];
    const errors = isInvalid[1];

    if (hasError) {
      this.setState({ formErrors: errors });
      return;
    }

    onMakeCopyIntoStorage(this.namesArray);
    this.setState({ formErrors: {} });
  };

  render() {
    const { formSettings, formErrors } = this.state;
    const {
      t,
      isLoadingData,
      isMaxProgress,
      selectedId,
      availableStorage,
    } = this.props;

    return (
      <>
        <SelectelSettings
          formSettings={formSettings}
          onChange={this.onChange}
          isLoadingData={isLoadingData}
          isError={formErrors}
          selectedStorage={availableStorage[selectedId]}
          t={t}
        />

        <div className="manual-backup_buttons">
          <Button
            label={t("MakeCopy")}
            onClick={this.onMakeCopy}
            primary
            isDisabled={!isMaxProgress || this.isDisabled}
            size="medium"
          />
          {!isMaxProgress && (
            <Button
              label={t("Copying")}
              onClick={() => console.log("click")}
              isDisabled={true}
              size="medium"
              style={{ marginLeft: "8px" }}
            />
          )}
        </div>
      </>
    );
  }
}
export default withTranslation("Settings")(SelectelStorage);
