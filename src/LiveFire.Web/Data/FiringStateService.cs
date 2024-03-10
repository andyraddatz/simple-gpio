using LiveFire.Web.Models;
using Microsoft.Extensions.Options;

namespace LiveFire.Web.Data;

public class FiringStateService : IDisposable
{
    private readonly GpioService _service;
    private readonly FiringSystemOptions _options;
    private PeriodicTimer? _timer;
    private readonly static TimeSpan heartbeatTickRate = TimeSpan.FromSeconds(1);

    public FiringStateService(IOptions<FiringSystemOptions> options, GpioService service)
    {
        _service = service;

        Pins = _service.PinMappings
            .Select(pm => new FiringSystemPin { PinMapping = pm, State = FiringState.Ready })
            .ToList();

        _options = options.Value;
        Acts = _options.Acts?.ToList();
    }

    public async Task OnCueChange(string address, bool isFiring)
    {
        if (NotifyCueStateChange is not null)
        {
            // TODO: deduce state from isFiring
            var state = FiringState.Firing;
            await NotifyCueStateChange.Invoke(address, state);
        }
    }

    public event Func<string, FiringState, Task>? NotifyCueStateChange;
    public event Func<int, FiringState, Task>? NotifyActStateChange;

    private List<FiringSystemPin>? Pins { get; set; }
    private List<FiringSystemAct>? Acts { get; set; }

    public Dictionary<int, FiringState>? GetActStates()
    {
        return Acts?.ToDictionary(a => a.ActNumber, a => a.State);
    }
    public Dictionary<string, FiringState>? GetCueStates()
    {
        return Pins?.ToDictionary(p => p.PinMapping.FiringAddress!, p => p.State);
    }
    public async Task StartAct(int actNumber)
    {
        if (_timer is null)
        {
            _timer = new(heartbeatTickRate);

            using (_timer)
            {
                var elapsedCount = 0;
                while (await _timer.WaitForNextTickAsync())
                {
                    elapsedCount += 1;

                    // TODO: fire any in ready state with firing time < elapsedCount

                }
            }
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
