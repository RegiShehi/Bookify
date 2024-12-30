﻿namespace Bookify.Domain.Apartments;

public record Money(decimal Amount, Currency Currency)
{
    public static Money operator +(Money first, Money second)
    {
        if (first.Currency != second.Currency) throw new InvalidOperationException("Currencies have to be equal");

        return first with { Amount = first.Amount + second.Amount };
    }

    public static Money Zero()
    {
        return new Money(0, Currency.None);
    }
}