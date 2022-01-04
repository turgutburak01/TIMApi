using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TIM.SDK.GLOGI;
using TIM.SDK.GLOGI.Executions;
using TIM.SDK.GLOGI.Helpers;
using TIM.SDK.GLOGI.Models;

namespace TIMApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimController : Controller
    {
        private readonly IConfiguration Configuration;
        private string _cacheUrl;

        private void AddToCache(string key, object value)
        {
            var redisSvc = new RedisCacheService();
            redisSvc.Set(key, value, 20);
        }

        public TimController(IConfiguration configuration)
        {
            Configuration = configuration;
            _cacheUrl = Configuration["CacheUrl"];
        }//

        private Indicator CreateIndicator(bool vibrate)
        {
            var buzzer = DisplayFactory.CreateBuzzer(Status.On, 100, 100, 1);
            var light = DisplayFactory.CreateLight(Color.Green, Status.On, 100, 100, 1);

            var vibration = DisplayFactory.CreateVibration(vibrate ? Status.On : Status.Off, 100, 100, 3);
            var indicator = DisplayFactory.InitializeIndicator(light, buzzer, vibration);
            return indicator;
        }//

        // GET: api/<TimController>
        [HttpGet]
        public Display DeviceCall(string id)
        {
            DeviceRequest model = new DeviceRequest();
            var cacheKey = id;
            var screen = DisplayFactory.InitializeScreen("loginscreen", "Login");
            //StateFactory<OperationDataModel>.InitializeStateMachine(_cacheUrl, cacheKey, DateTime.Now.AddMinutes(10), id, "GET");
            using (Display display = DisplayFactory.InitializeDisplay(DisplayMode.BarcodeScan, 0, screen, CreateIndicator(true)))
            {
                DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateTextElement
                    ("username"), 1);
                Screen s = DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateInputElement
                    ("txtusername"), 2);
                DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateTextElement
                    ("Password"), 3);
                DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateInputElement
                    ("txtpassword"), 4);
                //StateFactory<OperationDataModel>.AddOrUpdateStateMachineState(_cacheUrl, cacheKey, new StateModel
                // { DoNotCall = false, Request = null, Response = display, ScreenId = "GET" }, true, DateTime.Now.AddMinutes(10));
                return display;
            }
        }
        // POST api/<TimController>
        [HttpPost]
        public Display DeviceCall([FromBody] DeviceRequest model)
        {
            var screen = DisplayFactory.InitializeScreen("menuscreen", "KarTIM");
            string eventName = model.data.@event;
            var id = model.device.id;
            var cacheKey = "OperationKey_" + id;
            string userName = "";
            string password = "";

            if (model.data.input.Count > 0)
                userName = model.data.input[0].value;
            password = model.data.input[1].value;
            if (string.IsNullOrWhiteSpace(userName))
                userName = " ";
            if (string.IsNullOrWhiteSpace(password))
                password = " ";

            DeviceCall(userName);
            DeviceCall(password);

            using (Display display = DisplayFactory.InitializeDisplay(DisplayMode.BarcodeScan, 0, screen, CreateIndicator(false)))
            {
                if (eventName == "patient")
                {
                    IdTextInput(screen, display);
                    DisplayFactory.AddScreenElement(screen, DisplayFactory.CreateScreenElement
                            (ScreenElementType.Button, "logout", null, "Çıkış Yap", null, Functionkey.FN2, Font.S, Alignment.Right, null), 8);
                    DisplayFactory.AddScreenElement(screen, DisplayFactory.CreateScreenElement
                        (ScreenElementType.Button, "save", null, "Kaydet", null, Functionkey.FN1, Font.S, Alignment.Left, null), 8);
                    if (eventName == "save")
                        return PatientInfo();
                    else return display;
                }
                if (eventName == "treatment")
                {
                    IdTextInput(screen, display);
                    DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateTextElement("Reçete Numarası"), 3);
                    DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateInputElement("prescriptionnumber"), 4);
                    DisplayFactory.AddScreenElement(screen, DisplayFactory.CreateScreenElement
                        (ScreenElementType.Button, "logout", null, "Çıkış Yap", null, Functionkey.FN1, Font.S, Alignment.Right, null), 8);
                    return display;
                }
                if (eventName == "sample")
                {
                    IdTextInput(screen, display);
                    DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateTextElement("Numune Barkodu"), 3);
                    DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateInputElement("sampleqr"), 4);
                    DisplayFactory.AddScreenElement(screen, DisplayFactory.CreateScreenElement
                        (ScreenElementType.Button, "logout", null, "Çıkış Yap", null, Functionkey.FN1, Font.S, Alignment.Right, null), 8);
                    return display;
                }
                if (eventName == "food")
                {
                    IdTextInput(screen, display);
                    DisplayFactory.AddScreenElement(screen, DisplayFactory.CreateScreenElement
                           (ScreenElementType.Button, "logout", null, "Çıkış Yap", null, Functionkey.FN1, Font.S, Alignment.Right, null), 8);
                    return display;
                }
                if (eventName == "medicine")
                {
                    DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateTextElement("İlaç Barkodu"), 1);
                    DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateInputElement("medicineqr"), 2);
                    DisplayFactory.AddScreenElement(screen, DisplayFactory.CreateScreenElement
                        (ScreenElementType.Button, "logout", null, "Çıkış Yap", null, Functionkey.FN1, Font.S, Alignment.Right, null), 8);
                    return display;
                }
                if (eventName == "consumption")
                {
                    DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateTextElement("Ürün Barkodu"), 1);
                    DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateInputElement("consumptionqr"), 2);
                    DisplayFactory.AddScreenElement(screen, DisplayFactory.CreateScreenElement
                        (ScreenElementType.Button, "logout", null, "Çıkış Yap", null, Functionkey.FN1, Font.S, Alignment.Right, null), 8);
                    return display;
                }
                if (eventName == "logout")
                {
                    StateFactory<OperationDataModel>.RemoveStateFactory(_cacheUrl, cacheKey);
                    return DeviceCall(model.device.id);
                }
                if (eventName == "save") return PatientInfo();
            }
            return GetMainMenu(screen);
        }
        private Display GetMainMenu(Screen screen)
        {
            using (Display display = DisplayFactory.InitializeDisplay(DisplayMode.BarcodeScan, 0, screen, CreateIndicator(true)))
            {
                DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateButtonElement
                    ("patient", "Hasta Sorgula"), 1);

                DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateButtonElement
                ("treatment", "Tedavi Uygula"), 2);

                DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateButtonElement
                ("sample", "Numune Doğrula"), 3);

                DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateButtonElement
                ("food", "Yemek Dağıtım"), 4);

                DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateButtonElement
                ("medicine", "İlaç Sorgula"), 5);

                DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateButtonElement
                ("consumption", "Sarf Sorgula"), 6);
                return display;
            }
        }
        private Display PatientInfo()
        {
            var screen = DisplayFactory.InitializeScreen("patientscreen", "KarTIM");
            using (Display display = DisplayFactory.InitializeDisplay(DisplayMode.BarcodeScan, 0, screen, CreateIndicator(true)))
            {
                DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateTextElement("Hasta Bilgisi"), 1);
                return display;
            }
        }
        private Display IdTextInput(Screen screen, Display display)
        {
            DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateTextElement("Kimlik Numarası"), 1);

            DisplayFactory.AddScreenElement(screen, DisplayFactoryHelper.CreateInputElement("idnumber"), 2);

            return display;
        }
    }
}
