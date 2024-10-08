// See https://aka.ms/new-console-template for more information
using Lab1.Models;
using Lab1.Services;
using Lab1.Mappers;

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

var json = storeInfoService.StoreAsJson(products);
File.WriteAllText("productsInicial.json", json);

var priceMapper = new PriceMapper();

//Map price to euro using mapping function using Linq extension
var productsInEuro = priceMapper.LeiToEuro(products);
var jsonEuro = storeInfoService.StoreAsJson(productsInEuro);
File.WriteAllText("productsInEuro.json", jsonEuro);

var filteredProducts = priceMapper.FilterProductsByPrice(productsInEuro, 100, 250);
var jsonFiltered = storeInfoService.StoreAsJson(filteredProducts);
File.WriteAllText("productsFiltered.json", jsonFiltered);