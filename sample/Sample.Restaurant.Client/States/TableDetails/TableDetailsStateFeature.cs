﻿using Fluxor;

namespace Sample.Restaurant.Client.States.TableDetails;

public sealed class TableDetailsStateFeature : Feature<TableDetailsState>
{
    public override string GetName() => nameof(TableDetailsState);

    protected override TableDetailsState GetInitialState() => new();
}
