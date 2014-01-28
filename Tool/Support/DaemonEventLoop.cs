using System.Linq;
using OdjfsScraper.Database;

namespace OdjfsScraper.Tool.Support
{
    public class DaemonEventLoop
    {
        private readonly Entities _ctx;

        public DaemonEventLoop(Entities ctx)
        {
            _ctx = ctx;
            UpdateCounts();
            CurrentStep = 0;
            IsRunning = true;
        }

        public bool IsRunning { get; set; }
        public int CurrentStep { get; private set; }
        public int CountyCount { get; private set; }
        public int ChildCareCount { get; private set; }

        private int StepCount
        {
            get { return CountyCount + ChildCareCount; }
        }

        public int ChildCaresPerCounty
        {
            get
            {
                if (CountyCount == 0)
                {
                    return 0;
                }
                return ChildCareCount/CountyCount;
            }
        }

        public bool IsCountyStep
        {
            get
            {
                if (ChildCaresPerCounty == 0)
                {
                    return true;
                }
                return CurrentStep%ChildCaresPerCounty == 1;
            }
        }

        public bool IsChildCareStep
        {
            get { return !IsCountyStep; }
        }

        private void UpdateCounts()
        {
            CountyCount = _ctx.Counties.Count();
            ChildCareCount = _ctx.ChildCares.Count();
        }

        public bool NextStep()
        {
            CurrentStep = (CurrentStep + 1)%StepCount;
            if (CurrentStep == 0)
            {
                UpdateCounts();
            }
            return IsRunning;
        }
    }
}