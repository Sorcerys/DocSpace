import {
  getPaymentSettings,
  setLicense,
  acceptLicense,
} from "@docspace/common/api/settings";
import { makeAutoObservable } from "mobx";
import api from "@docspace/common/api";
import toastr from "client/toastr";

class PaymentStore {
  salesEmail = "sales@onlyoffice.com";
  helpUrl = "https://helpdesk.onlyoffice.com";
  buyUrl =
    "https://www.onlyoffice.com/enterprise-edition.aspx?type=buyenterprise";
  standaloneMode = true;
  currentLicense = {
    expiresDate: new Date(),
    trialMode: true,
  };

  paymentLink = null;
  accountLink = null;
  isLoading = false;
  totalPrice = 30;
  managersCount = 1;
  maxAvailableManagersCount = 999;
  minAvailableManagersCount = 1;
  paymentTariff = [];

  constructor() {
    makeAutoObservable(this);
  }

  setSalesEmail = async () => {
    const newSettings = await getPaymentSettings();
    const { salesEmail } = newSettings;
    this.salesEmail = salesEmail;
  };
  getSettingsPayment = async () => {
    const newSettings = await getPaymentSettings();
    const {
      buyUrl,
      salesEmail,
      currentLicense,
      standalone: standaloneMode,
      feedbackAndSupportUrl: helpUrl,
    } = newSettings;

    this.buyUrl = buyUrl;
    this.salesEmail = salesEmail;
    this.helpUrl = helpUrl;
    this.standaloneMode = standaloneMode;
    if (currentLicense) {
      if (currentLicense.date)
        this.currentLicense.expiresDate = new Date(currentLicense.date);

      if (currentLicense.trial)
        this.currentLicense.trialMode = currentLicense.trial;
    }

    return newSettings;
  };

  setPaymentsLicense = async (confirmKey, data) => {
    const response = await setLicense(confirmKey, data);

    this.acceptPaymentsLicense();
    this.getSettingsPayment();

    return response;
  };

  acceptPaymentsLicense = async () => {
    const response = await acceptLicense().then((res) => console.log(res));

    return response;
  };

  // ------------ For docspace -----------

  setPaymentAccount = async () => {
    try {
      const res = await api.portal.getPaymentAccount();

      if (res) {
        if (res.indexOf("error") === -1) {
          this.accountLink = res;
        } else {
          toastr.error(res);
        }
      } else {
        console.error(res);
      }
    } catch (e) {
      console.error(e);
    }
  };

  setPaymentLink = async (link) => {
    this.paymentLink = link;
  };
  updatePayment = async (adminCount) => {
    try {
      const res = await api.portal.updatePayment(adminCount);
      console.log("updatePayment", res);
      if (res !== true) {
        toastr.error("error");
      } else {
        toastr.success("the changes will be applied soon");
      }
    } catch (e) {
      toastr.error(e);
    }
  };

  setIsLoading = (isLoading) => {
    this.isLoading = isLoading;
  };

  setTotalPrice = (price) => {
    if (price > 0 && price !== this.totalPrice) this.totalPrice = price;
  };

  setManagersCount = (managers) => {
    this.managersCount = managers;
  };

  get isNeedRequest() {
    return this.managersCount > this.maxAvailableManagersCount;
  }

  get isLessCountThanAcceptable() {
    return this.managersCount < this.minAvailableManagersCount;
  }

  setRangeBound = (managerStep, min, max) => {
    this.minAvailableManagersCount = min;
    this.maxAvailableManagersCount = max;
  };

  sendPaymentRequest = async (email, userName, message) => {
    try {
      await api.portal.sendPaymentRequest(email, userName, message);
      toastr.success("Success");
    } catch (e) {
      toastr.error(e);
    }
  };

  // setPaymentTariff = async () => {
  //   try {
  //     const res = await api.portal.getPaymentTariff();
  //     if (res) {
  //       this.paymentTariff = res;
  //     }
  //   } catch (e) {}
  // };
}

export default PaymentStore;
