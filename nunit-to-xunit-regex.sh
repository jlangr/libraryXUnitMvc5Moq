#!/bin/sh

using Xunit;
using Assert = Xunit.Assert;


%s/\[Test\]/[Fact]/


%s/Assert\.That(\(.*\),\s*Is.EqualTo(\(.*\)))/Assert.Equal(\2, \1)/



Assert.That(Holding.IsBarcodeValid("ABC:X"), Is.False);

%s/Assert\.That(\(.*\),\s*Is.False)/Assert.False(\1)/


 Assert.That(holding, Is.Not.Null);

Assert.That(dictionary.LookUp("smelt"), Is.Null);

%s/Assert\.That(\(.*\),\s*Is.Null)/Assert.Null(\1)/
 
