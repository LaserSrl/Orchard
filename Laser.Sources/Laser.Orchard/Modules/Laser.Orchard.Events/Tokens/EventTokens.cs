using Laser.Orchard.Events.Models;
using Orchard;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Laser.Orchard.Events.Tokens
{
    public interface ITokenProvider : IEventHandler
    {
        void Describe(DescribeContext context);
        void Evaluate(EvaluateContext context);
    }

    public class EventTokens : ITokenProvider
    {
        private readonly ITokenizer _tokenizer;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Lazy<CultureInfo> _cultureInfo;

        public Localizer T { get; set; }

        public EventTokens(ITokenizer tokenizer, IWorkContextAccessor workContextAccessor)
        {
            _tokenizer = tokenizer;
            _workContextAccessor = workContextAccessor;
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentSite.SiteCulture));
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context)
        {
            context.For("EventList", T("Event List"), T("Tokens for Calendars (list shape)"))
                   .Token("StartDate", T("List Start Date"), T("Start date of the parent Calendar"))
                   .Token("EndDate", T("List End Date"), T("End date of the parent Calendar"));
        }

        public void Evaluate(EvaluateContext context)
        {
            context.For<CalendarPart>("EventList")
                .Token("StartDate", GetStartDate)
                .Token("EndDate", GetEndDate);
        }


        private string GetStartDate(CalendarPart part)
        {
            if (part == null)
                return null;
            else
                return _tokenizer.Replace(part.StartDate, new Dictionary<string, object>());
        }

        private string GetEndDate(CalendarPart part)
        {
            if (part == null) return null;

            DateTime startDate = Convert.ToDateTime(_tokenizer.Replace(part.StartDate, new Dictionary<string, object>()), _cultureInfo.Value);

            int duration = 1; //Valore standard da usare se la conversione fallisce
            int.TryParse(_tokenizer.Replace(part.NumDays, new Dictionary<string, object>()), out duration);
            if (duration <= 0) duration = 1;

            return startDate.AddDays(duration - 1).ToString(_cultureInfo.Value);
        }

    }
}