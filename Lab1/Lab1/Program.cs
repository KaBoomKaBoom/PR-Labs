// See https://aka.ms/new-console-template for more information
using Lab1.Models;
using Lab1.Services;

var requestSiteService = new RequestSiteService();
var htmlContent = await requestSiteService.GetSiteContent("https://darwin.md/telefoane");



var storeInfoService = new StoreInfoService();
List<Product>? products = storeInfoService.StoreInfo(htmlContent);

// Store additional info
foreach (var product in products)
{
    var htmlContentProduct = await requestSiteService.GetSiteContent(product.Link);
    storeInfoService.StoreAdditionalInfo(htmlContentProduct, product);
}

foreach (var product in products)
{
    Console.WriteLine($"Product: {product.Name}, Price: {product.Price}, Link: {product.Link}, Resolution: {product.Resolution}");
}