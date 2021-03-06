﻿using System;
using BearBonesMessaging.Serialisation;
using Example.Types;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Unit.Tests.Serialisation
{
	[TestFixture]
	public class DeserialisingAMessageWhenTheOriginalInterfaceIsUnavailable
	{
		IMessageSerialiser subject;
		string message;
		SuperMetadata originalObject;
        string contract;

        [SetUp]
		public void SetUp()
		{
			subject = new MessageSerialiser();
			originalObject = new SuperMetadata
				{
					CorrelationId = Guid.Parse("05C90FEB-5C10-4179-9FC0-D26DDA5FD1C6"),
					Contents = "My message contents",
					FilePath = "C:\\work\\message",
					HashValue = 123124512,
					MetadataName = "Mind the gap"
				};
			message = subject.Serialise(originalObject, out contract);
            contract = contract.Replace("Example.Types.IMetadataFile, Example.Types", "Pauls.IMum, Phils.Face");

            Console.WriteLine(message);
            Console.WriteLine(contract);
		}


		[Test]
		public void Should_return_nearest_available_type ()
		{
			var result = subject.DeserialiseByStack(message, contract);
            var baseType = result as IFile;

            Assert.That(baseType, Is.Not.Null, "Expected the base file type, but didn't get it -- was " + result.GetType());
        }
    }
}