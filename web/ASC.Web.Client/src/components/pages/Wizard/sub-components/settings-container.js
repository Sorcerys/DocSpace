import React from 'react';
import PropTypes from "prop-types";
import styled from 'styled-components';

import { 
  Box, 
  ComboBox,
  Text, 
  Link, 
  utils 
} from 'asc-web-components';

const { tablet } = utils.device;

const StyledContainer = styled(Box)`
  width: 311px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: min-content auto;
  grid-auto-columns: min-content;
  grid-row-gap: 12px;

  .title {
    white-space: nowrap;
  }

  .machine-name-value,
  .email-value {
    margin-left: 16px;
  }

  .drop-down {
    margin-left: 8px;
  }

  @media ${tablet} {
    width: 100%;
  }
`;

const SettingsContainer = ({
  selectLanguage, 
  selectTimezone, 
  languages, 
  timezones, 
  emailNeeded, 
  email, 
  emailOwner,
  t, 
  machineName, 
  onClickChangeEmail, 
  onSelectLanguageHandler, 
  onSelectTimezoneHandler
}) => {
  
  const titleEmail = !emailNeeded 
    ? <Text>{t('email')}</Text>
    : null
  
  const contentEmail = !emailNeeded 
    ? <Link 
        className="email-value" 
        type="action" 
        fontSize="13px" 
        fontWeight="600"
        isHovered={true} 
        onClick={onClickChangeEmail}
      >
        {email ? email : emailOwner}
      </Link>
    : null
  
  return (
    <StyledContainer>
      <Text fontSize="13px">{t('domain')}</Text>
      <Text className="machine-name-value" fontSize="13px" fontWeight="600">{machineName}</Text>

      {titleEmail}
      {contentEmail}

      <Text fontSize="13px">{t('language')}</Text>
      <ComboBox 
          className="drop-down" 
          options={languages}
          selectedOption={{
            key: selectLanguage.key,
            label: selectLanguage.label
          }} 
          noBorder={true}
          scaled={false}
          size='content'
          dropDownMaxHeight={300} 
          onSelect={onSelectLanguageHandler}
        />

      <Text className="title" fontSize="13px">{t('timezone')}</Text>
      <ComboBox 
          className="drop-down"
          options={timezones}
          selectedOption={{
            key: selectTimezone.key,
            label: selectTimezone.label 
          }}
          noBorder={true}
          dropDownMaxHeight={300}
          scaled={false}
          size='content'
          onSelect={onSelectTimezoneHandler}
        />
  
    </StyledContainer>
  );
}

SettingsContainer.propTypes = {
  selectLanguage: PropTypes.object.isRequired,
  selectTimezone: PropTypes.object.isRequired,
  languages: PropTypes.array.isRequired,
  timezones: PropTypes.array.isRequired,
  emailNeeded: PropTypes.bool.isRequired,
  emailOwner: PropTypes.string,
  t: PropTypes.func.isRequired,
  machineName: PropTypes.string.isRequired,
  email: PropTypes.string,
  onClickChangeEmail: PropTypes.func.isRequired,
  onSelectLanguageHandler: PropTypes.func.isRequired,
  onSelectTimezoneHandler: PropTypes.func.isRequired
}

export default SettingsContainer;