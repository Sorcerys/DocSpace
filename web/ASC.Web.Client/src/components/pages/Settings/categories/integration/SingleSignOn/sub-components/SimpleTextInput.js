import React from "react";
import { inject, observer } from "mobx-react";

import TextInput from "@appserver/components/text-input";

import StyledInputWrapper from "../styled-containers/StyledInputWrapper";

const SimpleTextInput = (props) => {
  const {
    hasError,
    isDisabled,
    maxWidth,
    name,
    placeholder,
    tabIndex,
    value,
    enableSso,
    onBlur,
    onFocus,
    onTextInputChange,
  } = props;

  return (
    <StyledInputWrapper maxWidth={maxWidth}>
      <TextInput
        className="field-input"
        hasError={hasError}
        isDisabled={isDisabled ?? !enableSso}
        name={name}
        onBlur={onBlur}
        onFocus={onFocus}
        onChange={onTextInputChange}
        placeholder={placeholder}
        scale
        tabIndex={tabIndex}
        value={value}
      />
    </StyledInputWrapper>
  );
};

export default inject(({ ssoStore }) => {
  const { enableSso, onBlur, onFocus, onTextInputChange } = ssoStore;

  return {
    enableSso,
    onBlur,
    onFocus,
    onTextInputChange,
  };
})(observer(SimpleTextInput));
