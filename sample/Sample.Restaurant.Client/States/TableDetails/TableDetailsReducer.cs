﻿using Fluxor;
using Lewee.Blazor.Fluxor;
using Sample.Restaurant.Client.States.TableDetails.Actions;

namespace Sample.Restaurant.Client.States.TableDetails;

internal static class TableDetailsReducer
{
    [ReducerMethod]
    public static TableDetailsState OnGetTableDetails(TableDetailsState state, GetTableDetailsAction action)
        => state.OnQuery<TableDetailsState, TableDetailsDto, GetTableDetailsAction>(action);

    [ReducerMethod]
    public static TableDetailsState OnGetTableDetailsSuccess(TableDetailsState state, GetTableDetailsSuccessAction action)
        => state.OnQuerySuccess<TableDetailsState, TableDetailsDto, GetTableDetailsSuccessAction>(action);

    [ReducerMethod]
    public static TableDetailsState OnGetTableDetailsError(TableDetailsState state, GetTableDetailsErrorAction action)
        => state.OnRequestError(action);

    [ReducerMethod]
    public static TableDetailsState OnOrderMenuItem(TableDetailsState state, OrderItemAction action)
        => state with { CorrelationId = action.CorrelationId, IsSaving = true };

    [ReducerMethod]
    public static TableDetailsState OnOrderMenuItemError(TableDetailsState state, OrderItemErrorAction action)
         => state.OnRequestError(action) with { IsSaving = false };

#pragma warning disable IDE0060 // Remove unused parameter
    [ReducerMethod]
    public static TableDetailsState OnOrderMenuItemCompleted(TableDetailsState state, OrderItemCompletedAction action)
        => state with { IsSaving = false };
#pragma warning restore IDE0060 // Remove unused parameter
}
