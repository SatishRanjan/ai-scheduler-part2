using ai_scheduler.src.actions;
using ai_scheduler.src.models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ai_scheduler.src
{
    public class ScheduleSerializer
    {
        public static string SerializeSchedules(VirtualWorld worldState)
        {
            if (worldState == null || worldState.ScheduleAndItsParticipatingConuntries == null || worldState.ScheduleAndItsParticipatingConuntries.Count == 0)
            {
                return "[]";
            }

            Stack<string> outputSchedulesStack = new Stack<string>();
            StringBuilder stringBuilder = new StringBuilder();

            VirtualWorld tempState = worldState;           
            while (tempState.Parent != null)
            {
                int lastIndex = tempState.ScheduleAndItsParticipatingConuntries.Count - 1;
                TemplateBase schedule = tempState.ScheduleAndItsParticipatingConuntries.ElementAt(lastIndex).Key;
                TransferTemplate transferAction = schedule as TransferTemplate;
                if (transferAction != null)
                {
                    stringBuilder.Append("(");
                    stringBuilder.Append(transferAction.TemplateName);
                    stringBuilder.Append(" ");
                    stringBuilder.Append(transferAction.ToCountry);
                    stringBuilder.Append(" ");
                    stringBuilder.Append(transferAction.FromCountry);
                    stringBuilder.Append(" ");
                    stringBuilder.Append("(");

                    int counter = transferAction.ResourceAndQuantityMapToTransfer.Count; ;
                    foreach (KeyValuePair<string, int> rq in transferAction.ResourceAndQuantityMapToTransfer)
                    {
                        stringBuilder.Append("(");
                        stringBuilder.Append(rq.Key);
                        stringBuilder.Append(" ");
                        stringBuilder.Append(rq.Value);
                        stringBuilder.Append(")");
                        if (--counter > 0)
                        {
                            stringBuilder.Append(" ");
                        }
                    }

                    stringBuilder.Append(")");
                    stringBuilder.Append(" ");
                    stringBuilder.Append("EU:" + tempState.ExpectedUtilityForSelf);
                    outputSchedulesStack.Push(stringBuilder.ToString());
                    stringBuilder = new StringBuilder();
                }


                TransformTemplate transformAction = schedule as TransformTemplate;
                if (transformAction != null)
                {
                    stringBuilder.Append("(");
                    stringBuilder.Append(transformAction.TemplateName);
                    stringBuilder.Append(" ");
                    stringBuilder.Append(transformAction.CountryName);
                    stringBuilder.Append(" ");
                    stringBuilder.Append("(");

                    int counter = transformAction.INPUTS.Count;
                    stringBuilder.Append("INPUTS");
                    stringBuilder.Append(" ");
                    foreach (KeyValuePair<string, int> rq in transformAction.INPUTS)
                    {
                        stringBuilder.Append("(");
                        stringBuilder.Append(rq.Key);
                        stringBuilder.Append(" ");
                        stringBuilder.Append(rq.Value);
                        stringBuilder.Append(")");
                        if (--counter > 0)
                        {
                            stringBuilder.Append(" ");
                        }
                    }

                    stringBuilder.Append(")");

                    stringBuilder.Append(" ");
                    stringBuilder.Append("(");
                    counter = transformAction.OUTPUTS.Count;
                    stringBuilder.Append("OUTPUTS");
                    stringBuilder.Append(" ");
                    foreach (KeyValuePair<string, int> rq in transformAction.OUTPUTS)
                    {
                        stringBuilder.Append("(");
                        stringBuilder.Append(rq.Key);
                        stringBuilder.Append(" ");
                        stringBuilder.Append(rq.Value);
                        stringBuilder.Append(")");
                        if (--counter > 0)
                        {
                            stringBuilder.Append(" ");
                        }
                    }

                    stringBuilder.Append(")");
                    stringBuilder.Append(" ");
                    stringBuilder.Append("EU(self):" + tempState.ExpectedUtilityForSelf);
                    outputSchedulesStack.Push(stringBuilder.ToString());
                    stringBuilder = new StringBuilder();
                }
                
                tempState = tempState.Parent;
            }

            stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            stringBuilder.Append("\n");
            while (outputSchedulesStack.Count > 0)
            {
                string item = outputSchedulesStack.Pop();
                stringBuilder.Append(item);                
                stringBuilder.Append("\n");
            }

            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }
    }
}
