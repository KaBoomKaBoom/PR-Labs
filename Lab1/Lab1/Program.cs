// See https://aka.ms/new-console-template for more information
using Lab1.Services;

var requestSiteService = new RequestSiteService();
var htmlContent = await requestSiteService.GetSiteContent();



var storeInfoService = new StoreInfoService();
storeInfoService.StoreInfo(htmlContent);
