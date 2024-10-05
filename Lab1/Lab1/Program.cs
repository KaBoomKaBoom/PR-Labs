// See https://aka.ms/new-console-template for more information
using Lab1.Services;

RequestSiteService service = new RequestSiteService();
await service.GetSiteContent();
