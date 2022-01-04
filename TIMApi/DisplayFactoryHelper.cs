using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TIM.SDK.GLOGI;
using TIM.SDK.GLOGI.Executions;

namespace TIMApi
{
        public static class DisplayFactoryHelper
        {
            public static ScreenElement CreateTextElement(string value)
            {
                return DisplayFactory.CreateScreenElement(ScreenElementType.Text, null, null, null, value, null, Font.M, Alignment.Center, null);

            }

            public static ScreenElement CreateInputElement(string id)
            {
                return DisplayFactory.CreateScreenElement(ScreenElementType.Input, id, null, null, null, null, Font.M, Alignment.Center, null);

            }

            public static ScreenElement CreateButtonElement(string id, string value)
            {
                return DisplayFactory.CreateScreenElement(ScreenElementType.Button, id, null, value, null, null, Font.M, Alignment.Center, null);

            }

        }
    }
