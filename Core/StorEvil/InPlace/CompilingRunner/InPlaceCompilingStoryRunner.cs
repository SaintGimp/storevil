﻿using System.Collections.Generic;
using System.Linq;
using StorEvil.Context;
using StorEvil.Core;
using StorEvil.Events;
using StorEvil.Parsing;

namespace StorEvil.InPlace
{
    public class InPlaceCompilingStoryRunner : IStoryHandler
    {
        private readonly IRemoteHandlerFactory _factory;
        private readonly IStoryFilter _filter;
        private readonly IEventBus _eventBus;
        private JobResult _result = new JobResult();


        public InPlaceCompilingStoryRunner(IRemoteHandlerFactory factory, IStoryFilter filter, IEventBus eventBus)

        {
            _factory = factory;
             _filter = filter;
            _eventBus = eventBus;
        }

        public JobResult HandleStories(IEnumerable<Story> stories)
        {
            var s = GetStories(stories);

            using (var remoteHandler = _factory.GetHandler(s, _eventBus))
                _result = remoteHandler.HandleStories(s.ToArray());

            Finished();
            return GetResult();
        }

        private IEnumerable<Story> GetStories(IEnumerable<Story> stories)
        {
            foreach (var story in  stories)
            {
                var scenarios = GetScenarios(story).ToArray();
                yield return new Story (story.Id, story.Summary, scenarios);
            }
        }

        private IEnumerable<IScenario> GetScenarios(Story story)
        {
            var filtered = story.Scenarios.Where(x => _filter.Include(story, x));

            return filtered.SelectMany(x => x.Preprocess()).Cast<IScenario>();
        }

        public void Finished()
        {
            _eventBus.Raise(new SessionFinished());
        }

        public JobResult GetResult()
        {
            return _result;
        }
    }
}