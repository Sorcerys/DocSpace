import { makeAutoObservable } from "mobx";
import {
  generateCerts,
  getCurrentSsoSettings,
  loadXmlMetadata,
  resetSsoForm,
  submitSsoForm,
  uploadXmlMetadata,
  validateCerts,
} from "@appserver/common/api/settings";
import toastr from "../helpers/toastr";
import { BINDING_POST, BINDING_REDIRECT } from "../helpers/constants";

class SsoFormStore {
  isSsoEnabled = false;

  set isSsoEnabled(value) {
    this.isSsoEnabled = value;
  }

  enableSso = false;

  uploadXmlUrl = "";

  spLoginLabel = "";

  onLoadXML = false;

  // idpSettings
  entityId = "";
  ssoUrlPost = "";
  ssoUrlRedirect = "";
  ssoBinding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST";
  sloUrlPost = "";
  sloUrlRedirect = "";
  sloBinding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST";
  nameIdFormat = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient";

  idp_certificate = "";
  idp_privateKey = null;
  idp_action = "signing";
  idp_certificates = [];

  // idpCertificateAdvanced
  idp_decryptAlgorithm = "http://www.w3.org/2001/04/xmlenc#aes128-cbc";
  // no checkbox for that
  ipd_decryptAssertions = false;
  idp_verifyAlgorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
  idp_verifyAuthResponsesSign = false;
  idp_verifyLogoutRequestsSign = false;
  idp_verifyLogoutResponsesSign = false;

  sp_certificate = "";
  sp_privateKey = "";
  sp_action = "signing";
  sp_certificates = [];

  // spCertificateAdvanced
  // null for some reason and no checkbox
  sp_decryptAlgorithm = "http://www.w3.org/2001/04/xmlenc#aes128-cbc";
  sp_encryptAlgorithm = "http://www.w3.org/2001/04/xmlenc#aes128-cbc";
  sp_encryptAssertions = false;
  sp_signAuthRequests = false;
  sp_signLogoutRequests = false;
  sp_signLogoutResponses = false;
  sp_signingAlgorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
  // sp_verifyAlgorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

  // Field mapping
  firstName = "";
  lastName = "";
  email = "";
  location = "";
  title = "";
  phone = "";

  hideAuthPage = false;

  // sp metadata
  sp_entityId = "";
  sp_assertionConsumerUrl = "";
  sp_singleLogoutUrl = "";

  // hide parts of form
  ServiceProviderSettings = false;
  idp_showAdditionalParameters = true;
  sp_showAdditionalParameters = true;
  SPMetadata = false;
  idp_isModalVisible = false;
  sp_isModalVisible = false;
  confirmationDisableModal = false;
  confirmationResetModal = false;

  // errors
  uploadXmlUrlHasError = false;
  spLoginLabelHasError = false;

  entityIdHasError = false;
  ssoUrlPostHasError = false;
  ssoUrlRedirectHasError = false;
  sloUrlPostHasError = false;
  sloUrlRedirectHasError = false;

  firstNameHasError = false;
  lastNameHasError = false;
  emailHasError = false;
  locationHasError = false;
  titleHasError = false;
  phoneHasError = false;

  sp_entityIdHasError = false;
  sp_assertionConsumerUrlHasError = false;
  sp_singleLogoutUrlHasError = false;

  // error messages
  //uploadXmlUrlErrorMessage = null;
  spLoginLabelErrorMessage = null;

  entityIdErrorMessage = null;
  ssoUrlPostErrorMessage = null;
  ssoUrlRedirectErrorMessage = null;
  sloUrlPostErrorMessage = null;
  sloUrlRedirectErrorMessage = null;

  firstNameErrorMessage = null;
  lastNameErrorMessage = null;
  emailErrorMessage = null;
  locationErrorMessage = null;
  titleErrorMessage = null;
  phoneErrorMessage = null;

  sp_entityIdErrorMessage = null;
  sp_assertionConsumerUrlErrorMessage = null;
  sp_singleLogoutUrlErrorMessage = null;

  isSubmitLoading = false;

  constructor() {
    makeAutoObservable(this);
  }

  onPageLoad = async () => {
    try {
      const response = await getCurrentSsoSettings();
      this.isSsoEnabled = response.enableSso;
      this.setFieldsFromObject(response);
    } catch (err) {
      console.log(err);
    }
  };

  onSsoToggle = () => {
    if (!this.enableSso) {
      this.enableSso = true;
      this.ServiceProviderSettings = true;
    } else {
      this.enableSso = false;
    }

    for (let key in this) {
      if (key.includes("ErrorMessage")) this[key] = null;
    }
  };

  onTextInputChange = (e) => {
    this[e.target.name] = e.target.value;
  };

  onBindingChange = (e) => {
    this[e.target.name] = e.target.value;
  };

  onComboBoxChange = (option, field) => {
    this[field] = option.key;
  };

  onHideClick = (e, label) => {
    this[label] = !this[label];
  };

  onCheckboxChange = (e) => {
    this[e.target.name] = e.target.checked;
  };

  onOpenIdpModal = () => {
    this.idp_isModalVisible = true;
  };

  onOpenSpModal = () => {
    this.sp_isModalVisible = true;
  };

  onCloseModal = (e, modalVisible) => {
    this[modalVisible] = false;
  };

  onModalComboBoxChange = (option) => {
    this.spCertificateUsedFor = option.key;
  };

  onBlur = (e) => {
    const field = e.target.name;
    const value = e.target.value;

    this.setErrors(field, value);
  };

  onFocus = (e) => {
    const field = e.target.name;
    const fieldError = `${field}HasError`;
    const fieldErrorMessage = `${field}ErrorMessage`;
    this[fieldError] = false;
    this[fieldErrorMessage] = null;
  };

  disableSso = () => {
    this.isSsoEnabled = false;
  };

  openConfirmationDisableModal = () => {
    this.confirmationDisableModal = true;
  };

  openResetModal = () => {
    this.confirmationResetModal = true;
  };

  onConfirmDisable = () => {
    this.disableSso();
    this.onSsoToggle();
    this.confirmationDisableModal = false;
  };

  onConfirmReset = () => {
    this.resetForm();
    this.disableSso();
    this.ServiceProviderSettings = false;
    this.SPMetadata = false;
    this.confirmationResetModal = false;
  };

  onLoadXmlMetadata = async () => {
    const data = { url: this.uploadXmlUrl };

    try {
      this.onLoadXML = true;
      const response = await loadXmlMetadata(data);
      this.setFieldsFromMetaData(response.meta);
      this.onLoadXML = false;
    } catch (err) {
      this.onLoadXML = false;
      toastr.error(err);
      console.error(err);
    }
  };

  onUploadXmlMetadata = async (file) => {
    if (!file.type.includes("text/xml")) return console.log("invalid format");

    const data = new FormData();
    data.append("file", file);

    try {
      this.onLoadXML = true;
      const response = await uploadXmlMetadata(data);
      this.setFieldsFromObject(response);
      this.onLoadXML = false;
    } catch (err) {
      this.onLoadXML = false;
      toastr.error(err);
      console.error(err);
    }
  };

  validateCertificate = async (crts) => {
    const data = { certs: crts };

    try {
      return await validateCerts(data);
    } catch (err) {
      toastr.error(err);
      console.error(err);
    }
  };

  generateCertificate = async () => {
    try {
      const response = await generateCerts();
      this.setGeneratedCertificate(response);
    } catch (err) {
      toastr.error(err);
      console.error(err);
    }
  };

  getSettings = () => {
    const ssoUrl =
      this.ssoBinding === "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"
        ? this.ssoUrlPost
        : this.ssoUrlRedirect;
    const sloUrl =
      this.sloBinding === "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"
        ? this.sloUrlPost
        : this.sloUrlRedirect;

    return {
      enableSso: this.enableSso,
      spLoginLabel: this.spLoginLabel,
      idpSettings: {
        entityId: this.entityId,
        ssoUrl: ssoUrl,
        ssoBinding: this.ssoBinding,
        sloUrl: sloUrl,
        sloBinding: this.sloBinding,
        nameIdFormat: this.nameIdFormat,
      },
      idpCertificates: this.idp_certificates,
      idpCertificateAdvanced: {
        verifyAlgorithm: this.idp_verifyAlgorithm,
        verifyAuthResponsesSign: this.idp_verifyAuthResponsesSign,
        verifyLogoutRequestsSign: this.idp_verifyLogoutRequestsSign,
        verifyLogoutResponsesSign: this.idp_verifyLogoutResponsesSign,
        decryptAlgorithm: this.idp_decryptAlgorithm,
        decryptAssertions: false,
      },
      spCertificates: this.sp_certificates,
      spCertificateAdvanced: {
        decryptAlgorithm: this.sp_decryptAlgorithm,
        signingAlgorithm: this.sp_signingAlgorithm,
        signAuthRequests: this.sp_signAuthRequests,
        signLogoutRequests: this.sp_signLogoutRequests,
        signLogoutResponses: this.sp_signLogoutResponses,
        encryptAlgorithm: this.sp_encryptAlgorithm,
        encryptAssertions: this.sp_encryptAssertions,
      },
      fieldMapping: {
        firstName: this.firstName,
        lastName: this.lastName,
        email: this.email,
        title: this.title,
        location: this.location,
        phone: this.phone,
      },
      hideAuthPage: this.hideAuthPage,
    };
  };
  onSubmit = async (t) => {
    const settings = this.getSettings();
    const data = { serializeSettings: JSON.stringify(settings) };

    this.isSubmitLoading = true;

    try {
      await submitSsoForm(data);
      toastr.success(t("Settings:SuccessfullySaveSettingsMessage"));
      this.isSubmitLoading = false;
    } catch (err) {
      toastr.error(err);
      console.error(err);
      this.isSubmitLoading = false;
    }
  };

  resetForm = async () => {
    try {
      const response = await resetSsoForm();

      this.setFieldsFromObject(response);
    } catch (err) {
      toastr.error(err);
      console.error(err);
    }
  };

  setFieldsFromObject = (object) => {
    for (let key of Object.keys(object)) {
      if (typeof object[key] !== "object") {
        this[key] = object[key];
      } else {
        let prefix = "";

        if (key === "idpSettings") {
          this.setSsoUrls(object[key]);
          this.setSloUrls(object[key]);
        }

        if (key !== "fieldMapping" && key !== "idpSettings") {
          prefix = key.includes("idp") ? "idp_" : "sp_";
        }

        if (Array.isArray(object[key])) {
          this[`${prefix}certificates`] = object[key].slice();
        } else {
          for (let field of Object.keys(object[key])) {
            this[`${prefix}${field}`] = object[key][field];
          }
        }
      }
    }
  };

  setSsoUrls = (o) => {
    switch (o.ssoBinding) {
      case BINDING_POST:
        this.ssoUrlPost = o.ssoUrl;
        break;
      case BINDING_REDIRECT:
        this.ssoUrlRedirect = o.ssoUrl;
    }
  };

  setSloUrls = (o) => {
    switch (o.sloBinding) {
      case BINDING_POST:
        this.sloUrlPost = o.ssoUrl;
        break;
      case BINDING_REDIRECT:
        this.sloUrlRedirect = o.ssoUrl;
    }
  };

  setFieldsFromMetaData = async (meta) => {
    if (meta.entityID) {
      this.entityId = meta.entityID;
    }

    if (meta.singleSignOnService) {
      this.ssoUrlPost =
        meta.singleSignOnService[
          "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"
        ];
      this.ssoUrlRedirect =
        meta.singleSignOnService[
          "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"
        ];
    }

    if (meta.singleLogoutService) {
      this.sloBinding = meta.singleLogoutService.binding;
      if (
        meta.singleLogoutService.binding ===
        "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"
      ) {
        this.sloUrlRedirect = meta.singleLogoutService.location;
      }

      if (
        meta.singleLogoutService.binding ===
        "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"
      ) {
        this.sloUrlPost = meta.singleLogoutService.location;
      }
    }

    if (meta.nameIDFormat) {
      this.nameIdFormat = meta.nameIDFormat;
    }

    if (meta.certificate) {
      let data = [];

      if (meta.certificate.signing) {
        if (Array.isArray(meta.certificate.signing)) {
          meta.certificate.signing = this.getUniqueItems(
            meta.certificate.signing
          ).reverse();
          meta.certificate.signing.forEach((signingCrt) => {
            data.push({
              crt: signingCrt.trim(),
              key: null,
              action: "verification",
            });
          });
        } else {
          data.push({
            crt: meta.certificate.signing.trim(),
            key: null,
            action: "verification",
          });
        }
      }

      const newCertificates = await this.validateCertificate(data);

      newCertificates.map((cert) => {
        this.idp_certificates = [...this.idp_certificates, cert];

        if (cert.action === "verification") {
          this.idp_verifyAuthResponsesSign = true;
          this.idp_verifyLogoutRequestsSign = true;
        }
        if (cert.action === "decrypt") {
          this.idp_verifyLogoutResponsesSign = true;
        }
        if (cert.action === "verification and decrypt") {
          this.idp_verifyAuthResponsesSign = true;
          this.idp_verifyLogoutRequestsSign = true;
          this.idp_verifyLogoutResponsesSign = true;
        }
      });
    }
  };

  getUniqueItems = (array) => {
    return array.filter((item, index, array) => array.indexOf(item) == index);
  };

  onEditClick = (e, certificate, prefix) => {
    this[`${prefix}_certificate`] = certificate.crt;
    this[`${prefix}_privateKey`] = certificate.key;
    this[`${prefix}_action`] = certificate.action;
    this[`${prefix}_isModalVisible`] = true;
  };

  onDeleteClick = (e, crtToDelete, prefix) => {
    this[`${prefix}_certificates`] = this[`${prefix}_certificates`].filter(
      (certificate) => certificate.crt !== crtToDelete
    );
  };

  addCertificateToForm = async (e, prefix) => {
    const action = this[`${prefix}_action`];
    const crt = this[`${prefix}_certificate`];
    const key = this[`${prefix}_privateKey`];

    const data = [
      {
        crt: crt,
        key: key,
        action: action,
      },
    ];

    try {
      const newCertificates = await this.validateCertificate(data);
      newCertificates.map((cert) => {
        this[`${prefix}_certificates`] = [
          ...this[`${prefix}_certificates`],
          cert,
        ];
      });
      this.onCloseModal(e, `${prefix}_isModalVisible`);
    } catch (err) {
      toastr.error(err);
      console.error(err);
    }
  };

  setGeneratedCertificate = (certificateObject) => {
    this.sp_certificate = certificateObject.crt;
    this.sp_privateKey = certificateObject.key;
  };

  setErrors = (field, value) => {
    if (typeof value === "boolean") return;

    const fieldError = `${field}HasError`;
    const fieldErrorMessage = `${field}ErrorMessage`;

    try {
      this.validate(value);
      this[fieldError] = false;
      this[fieldErrorMessage] = null;
    } catch (err) {
      this[fieldError] = true;
      this[fieldErrorMessage] = err.message;
    }
  };

  validate = (string) => {
    if (string.trim().length === 0) throw new Error("EmptyFieldErrorMessage");
    else return true;
  };

  downloadMetadata = async () => {
    window.open("/api/2.0/sso/metadata", "_blank");
  };

  get hasErrors() {
    for (let key in this) {
      if (key.includes("ErrorMessage") && this[key] !== null) return true;
    }
    return false;
  }
}

export default SsoFormStore;
