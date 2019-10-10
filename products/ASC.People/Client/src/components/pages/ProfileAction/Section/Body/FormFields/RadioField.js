import React from "react";
import isEqual from "lodash/isEqual";
import { FieldContainer, RadioButtonGroup } from "asc-web-components";

class RadioField extends React.Component {
  shouldComponentUpdate(nextProps) {
    return !isEqual(this.props, nextProps);
  }

  render() {
    console.log("RadioField render");

    const {
      isRequired,
      hasError,
      labelText,

      radioName,
      radioValue,
      radioOptions,
      radioIsDisabled,
      radioOnChange,

      tooltipId,
      tooltipEvent,
      iconButton
    } = this.props;

    return (
      <FieldContainer
        isRequired={isRequired}
        hasError={hasError}
        labelText={labelText}
        tooltipId={tooltipId}
        tooltipEvent={tooltipEvent}
        iconButton={iconButton}
      >
        <RadioButtonGroup
          name={radioName}
          selected={radioValue}
          options={radioOptions}
          isDisabled={radioIsDisabled}
          onClick={radioOnChange}
          className="radio-group"
        />
      </FieldContainer>
    );
  }
}

export default RadioField;
