﻿using Nancy;
using Nancy.Extensions;
using Nancy.Responses;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using OpenHomeServer.Server.Plugins.Ripper.Domain;
using OpenHomeServer.Server.Storage;
using System.Collections.Generic;
using Nancy.Validation;

namespace OpenHomeServer.Server.Plugins.Ripper
{
    public class RipperModule : NancyModule
    {
        public RipperModule(RipperService service, Persister<RipperSettings> persister)
            : base("ripper")
        {
            Get["/"] = x => View["index.cshtml", GetRipperViewModel(service)];
            Post["/Cancel"] = x => 
            {
                service.CancelRipping();
                if (Context.IsAjaxRequest())
                {
                    return "ok";
                } 
                return new RedirectResponse(ModulePath);
            };
            Post["/selectAlbum"] = x =>
            {
                service.SelectAlbum(Request.Form.albumId);

                if (Context.IsAjaxRequest())
                {
                    return "ok";
                }
                return new RedirectResponse(ModulePath);
            };
            Get["/settings"] = x => View["settings.cshtml", new RipperSettingsViewModel { Title = "Ripper Settings", Settings = persister.Get() }];
            Post["/settings"] = x => 
            {
                if (Request.Form.Reset)
                {
                    persister.Save(new RipperSettings());
                }
                else
                {
                    var currentSettings = persister.Get();
                    this.BindToAndValidate(currentSettings);
                    if (this.ModelValidationResult.IsValid)
                    {
                        persister.Save(currentSettings);
                    }
                    else
                    {
                        return View["settings.cshtml", new RipperSettingsViewModel { Title = "Ripper Settings", Settings = currentSettings }];
                    }
                }
                return new RedirectResponse("settings");
            };
        }

        private RipperViewModel GetRipperViewModel(RipperService service)
        {
            var status = service.GetCurrentStatus();

            return new RipperViewModel
            {
                Title="Ripper",
                View = status == null ? "null" : JsonConvert.SerializeObject(status)
            };
        }
    }

    public class RipperViewModel
    {
        public string Title { get; set; }
        public string View { get; set; }
    }

    public class RipperSettingsViewModel
    {
        public string Title { get; set; }
        public RipperSettings Settings { get; set; }
    }
}
