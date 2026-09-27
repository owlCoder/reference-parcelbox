using Microsoft.AspNetCore.Components;
using ParcelBox.Web.Clients;
using ParcelBox.Web.Contracts;
using ParcelBox.Web.ViewModels;

namespace ParcelBox.Web.Components.Pages;

public partial class Home
{
    [Inject]
    private ParcelBoxApiClient Api { get; set; } = default!;

    [Inject]
    private LockerControllerClient LockerController { get; set; } = default!;

    [Inject]
    private MessageGatewayClient MessageGateway { get; set; } = default!;

    private readonly RegisterParcelForm _registerForm = new();
    private readonly PickupForm _pickupForm = new();
    private readonly List<ActivityEntry> _activity = [];

    private ParcelDetails? _currentParcel;
    private List<CompartmentDetails> _compartments = [];
    private List<SentMessage> _messages = [];
    private LockerMode? _lockerMode;
    private MessageGatewayMode? _messageMode;
    private bool _apiOnline;
    private bool _lockerOnline;
    private bool _messageOnline;
    private bool _busy;

    protected override async Task OnInitializedAsync()
    {
        await RefreshDashboardAsync();
    }

    private Task RegisterAsync()
    {
        return ExecuteAsync(async () =>
        {
            var result = await Api.RegisterAsync(
                new RegisterParcelRequest(
                    _registerForm.TrackingCode,
                    _registerForm.RecipientPhone,
                    _registerForm.Size));

            if (result.Value is null)
            {
                AddActivity($"Registration failed: {result.Error}");
                return;
            }

            _currentParcel = result.Value;
            _pickupForm.TrackingCode = result.Value.TrackingCode;
            AddActivity($"Parcel {result.Value.TrackingCode} registered as {result.Value.Size}.");
        });
    }

    private Task StoreAsync()
    {
        if (_currentParcel is null)
        {
            return Task.CompletedTask;
        }

        return ExecuteAsync(async () =>
        {
            var result = await Api.StoreAsync(_currentParcel.Id);

            if (result.Value is null)
            {
                AddActivity($"Store failed: {result.Error}");
                await RefreshOperationalStateAsync();
                return;
            }

            AddActivity(
                $"Parcel stored in {result.Value.LockerCode}/{result.Value.CompartmentNumber}; message {result.Value.MessageDelivery}.");

            await RefreshCurrentParcelAsync();
            await RefreshOperationalStateAsync();
        });
    }

    private Task PickupAsync()
    {
        return ExecuteAsync(async () =>
        {
            var error = await Api.PickupAsync(_pickupForm.TrackingCode, _pickupForm.Code);

            if (error is not null)
            {
                AddActivity($"Pickup failed: {error}");
                await RefreshOperationalStateAsync();
                return;
            }

            AddActivity($"Parcel {_pickupForm.TrackingCode} picked up successfully.");
            await RefreshCurrentParcelAsync();
            await RefreshOperationalStateAsync();
        });
    }

    private Task RefreshDashboardAsync()
    {
        return ExecuteAsync(async () =>
        {
            await RefreshOperationalStateAsync();

            if (_currentParcel is not null)
            {
                await RefreshCurrentParcelAsync();
            }
        });
    }

    private async Task RefreshOperationalStateAsync()
    {
        var compartments = await Api.ListCompartmentsAsync();

        _compartments = compartments.Value?.ToList() ?? [];
        _lockerMode = await LockerController.GetModeAsync();
        _messageMode = await MessageGateway.GetModeAsync();
        _messages = (await MessageGateway.ListMessagesAsync()).ToList();

        _apiOnline = await Api.IsHealthyAsync();
        _lockerOnline = await LockerController.IsHealthyAsync();
        _messageOnline = await MessageGateway.IsHealthyAsync();
    }

    private async Task RefreshCurrentParcelAsync()
    {
        if (_currentParcel is null)
        {
            return;
        }

        var result = await Api.GetAsync(_currentParcel.Id);

        if (result.Value is not null)
        {
            _currentParcel = result.Value;
        }
    }

    private Task SetLockerModeAsync(LockerMode mode)
    {
        return ExecuteAsync(async () =>
        {
            var changed = await LockerController.SetModeAsync(mode);
            _lockerMode = await LockerController.GetModeAsync();
            _lockerOnline = await LockerController.IsHealthyAsync();

            AddActivity(changed
                ? $"Locker controller switched to {mode}."
                : "Locker controller mode change failed.");
        });
    }

    private Task SetMessageModeAsync(MessageGatewayMode mode)
    {
        return ExecuteAsync(async () =>
        {
            var changed = await MessageGateway.SetModeAsync(mode);
            _messageMode = await MessageGateway.GetModeAsync();
            _messageOnline = await MessageGateway.IsHealthyAsync();

            AddActivity(changed
                ? $"Message gateway switched to {mode}."
                : "Message gateway mode change failed.");
        });
    }

    private Task ClearMessagesAsync()
    {
        return ExecuteAsync(async () =>
        {
            var cleared = await MessageGateway.ClearMessagesAsync();
            _messages = (await MessageGateway.ListMessagesAsync()).ToList();

            AddActivity(cleared
                ? "Message gateway inbox cleared."
                : "Message gateway inbox could not be cleared.");
        });
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        if (_busy)
        {
            return;
        }

        _busy = true;

        try
        {
            await action();
        }
        finally
        {
            _busy = false;
        }
    }

    private void AddActivity(string message)
    {
        _activity.Insert(0, new ActivityEntry(DateTimeOffset.Now, message));

        if (_activity.Count > 12)
        {
            _activity.RemoveAt(_activity.Count - 1);
        }
    }

    private void ClearActivity()
    {
        _activity.Clear();
    }

    private void UsePickupCode(string pickupCode)
    {
        _pickupForm.Code = pickupCode;
        AddActivity("Pickup code selected from the message gateway inbox.");
    }
}
