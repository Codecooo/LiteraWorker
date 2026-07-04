using System.Net.Http.Headers;
using LiteraWorker.Core.Networks.SignalRClient;
using LiteraWorker.Core.Services;
using LiteraWorker.Core.Services.ApiService;
using LiteraWorker.Core.Services.Auth;
using LiteraWorker.Core.Services.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;



var services = new ServiceCollection();
services.AddCoreServices();
