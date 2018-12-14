using System;
using BearBonesMessaging.Serialisation;
using Example.Types;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Unit.Tests.Serialisation
{
	[TestFixture]
	public class InterfaceStackTests
	{
		[Test]
		public void Should_get_correct_stack_for_complex_types ()
		{
			var source = new SuperMetadata();

			var result = InterfaceStack.Of(source);
			Console.WriteLine(result);
			Assert.That(result,
				// Windows spits out this:
				Is.EqualTo("Example.Types.IMetadataFile;" +
				           "Example.Types.IFile;" +
				           "Example.Types.IHash;" +
				           "Example.Types.IPath;" +
				           "Example.Types.IMsg")
						   
				.Or
				// Mono spits out this:
				.EqualTo("Example.Types.IMetadataFile;" +
				           "Example.Types.IFile;" +
				           "Example.Types.IPath;" +
				           "Example.Types.IHash;" +
				           "Example.Types.IMsg"));
		}
	}
}
