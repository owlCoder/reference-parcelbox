using Microsoft.AspNetCore.Components;
using ParcelBox.Web.Contracts;
using ParcelBox.Web.Enums;
using ParcelBox.Web.Interfaces;
using ParcelBox.Web.ViewModels;

namespace ParcelBox.Web.Components.Pages;

public partial class Home
{
    [Inject]
    private IParcelBoxApiClient Api { get; set; } = default!;

    [Inject]
    private ILockerControllerClient LockerController { get; set; } = default!;

    [Inject]
    private IMessageGatewayClient MessageGateway { get; set; } = default!;

    private readonly RegisterParcelForm _registerForm = new();
    private readonly PickupForm _pickupForm = new();
    private readonly List<ActivityEntry> _activity = [];

    private ParcelDetailsDto? _currentParcel;
    private List<CompartmentDetailsDto> _compartments = [];
    private List<SentMessageDto> _messages = [];
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
                new RegisterParcelRequestDto(
                    _registerForm.TrackingCode,
                    _registerForm.RecipientPhone,
                    _registerForm.Size));

            if (result.IsFailure)
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

            if (result.IsFailure)
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
            var pickup = await Api.PickupAsync(_pickupForm.TrackingCode, _pickupForm.Code);

            if (pickup.IsFailure)
            {
                AddActivity($"Pickup failed: {pickup.Error}");
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

        _compartments = compartments.IsSuccess ? compartments.Value : [];
        var lockerMode = await LockerController.GetModeAsync();
        _lockerMode = lockerMode.IsSuccess ? lockerMode.Value : null;
        var messageMode = await MessageGateway.GetModeAsync();
        _messageMode = messageMode.IsSuccess ? messageMode.Value : null;
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

        if (result.IsSuccess)
        {
            _currentParcel = result.Value;
        }
    }

    private Task SetLockerModeAsync(LockerMode mode)
    {
        return ExecuteAsync(async () =>
        {
            var changed = await LockerController.SetModeAsync(mode);
            var lockerMode = await LockerController.GetModeAsync();
            _lockerMode = lockerMode.IsSuccess ? lockerMode.Value : null;
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
            var messageMode = await MessageGateway.GetModeAsync();
            _messageMode = messageMode.IsSuccess ? messageMode.Value : null;
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
