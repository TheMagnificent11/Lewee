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
}
