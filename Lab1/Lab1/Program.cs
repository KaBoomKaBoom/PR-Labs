// See https://aka.ms/new-console-template for more information
using Lab1.Models;
using Lab1.Services;
using Lab1.Mappers;
using System.Text.Json;
using RabbitMQ.Stream.Client;
using System.Net;

var sendToRMQ = new SendToRMQ();
var requestSiteService = new RequestSiteService();
List<Product>? productsAll = new List<Product>();

var streamSystem = await StreamSystem.Create(new StreamSystemConfig());
var count = 1;
for (int i = 2; i < 5; i++)
{
    var htmlContent = await requestSiteService.GetSiteContent($"https://enter.online/telefoane/smartphone-uri?page={i}");

    var storeInfoService = new StoreInfoService();
    List<Product>? products = storeInfoService.StoreInfo(htmlContent);

    // Store additional info
    foreach (var product in products)
    {
        var htmlContentProduct = await requestSiteService.GetSiteContent(product.Link);
        storeInfoService.StoreAdditionalInfo(htmlContentProduct, product);

        sendToRMQ.Send(streamSystem, JsonSerializer.Serialize(product));
        Console.WriteLine($"Sent message: product {count}");
        count++;
    }

    //sendToRMQ.Send(streamSystem, JsonSerializer.Serialize(products));

    productsAll.AddRange(products);
}
var serializationService = new SerializationService();

//Inicial data extracted from site
// var json = storeInfoService.StoreAsJson(products);
var json = serializationService.SerializeListToJson(productsAll);
var xml = serializationService.SerializeListToXML(productsAll);
File.WriteAllText("productsInicial.json", json);
//File.WriteAllText("productsInicial.xml", xml);

var priceMapper = new PriceMapper();

// Modified price to euro
// Map price to euro using mapping function using Linq extension
var productsInEuro = priceMapper.LeiToEuro(productsAll);
//var jsonEuro = storeInfoService.StoreAsJson(productsInEuro);
var jsonEuro = serializationService.SerializeListToJson(productsInEuro);
File.WriteAllText("productsInEuro.json", jsonEuro);
// var xmlEuro = serializationService.SerializeListToXML(productsInEuro);
// File.WriteAllText("productsInEuro.xml", xmlEuro);


// //Filtered products by price
// var filteredProducts = priceMapper.FilterProductsByPrice(productsInEuro, 100, 250);
// //var jsonFiltered = storeInfoService.StoreAsJson(filteredProducts);
// //File.WriteAllText("productsFiltered.json", jsonFiltered);

// //Filtered products + atached total price and time
// var filteredProductsTotalPrice = storeInfoService.StoreProductsWithTotalPrice(filteredProducts, priceMapper.SumPrices(filteredProducts)); 
// //var jsonFilteredTotalPrice = JsonSerializer.Serialize(filteredProductsTotalPrice);
// var jsonFilteredTotalPrice = serializationService.SerializeListToJson(filteredProductsTotalPrice);
// var xmlFilteredTotalPrice = serializationService.SerializeListToXML(filteredProductsTotalPrice);
// File.WriteAllText("productsFilteredTotalPrice.json", jsonFilteredTotalPrice);
// File.WriteAllText("productsFilteredTotalPrice.xml", xmlFilteredTotalPrice);

// //Custom serialization/deserialization
// var customSerializationService = new CustomSerializationService();
// var customSerialized = customSerializationService.SerializeList(products);
// File.WriteAllText("productsCustomSerialized.txt", customSerialized);

// List<Product> customDeserialized = customSerializationService.DeserializeList<Product>(customSerialized);
// Console.WriteLine(customDeserialized.Count);
// foreach (var product in customDeserialized)
// {
//     Console.WriteLine("Product");
//     Console.WriteLine(product.Name);
//     Console.WriteLine(product.Price);
//     Console.WriteLine(product.Link);
//     Console.WriteLine(product.Resolution + "\n");
// }
