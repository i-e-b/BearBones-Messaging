using System;
using System.Threading;
using BearBonesMessaging;
using BearBonesMessaging.Routing;
using BearBonesMessaging.Serialisation;
using Example.Types;
using Messaging.Base.Integration.Tests.Helpers;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Integration.Tests
{
    [TestFixture]
    public class EndToEnd_WithRootedContracts
    {
        SuperMetadata testMessage;
        IMessagingBase messaging;

        [OneTimeSetUp]
        public void A_configured_messaging_base()
        {
            messaging = new MessagingBaseConfiguration()
                .WithDefaults()
                .WithContractRoot<IMsg>()
                .WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettings())
                .WithApplicationGroupName("app-group-name")
                .GetMessagingBase();

            testMessage = new SuperMetadata
            {
                CorrelationId = Guid.NewGuid(),
                Contents = "These are my ||\"\\' ' contents: ⰊⰄⰷἚ𐰕𐰑ꔘⶤعبػػ↴↳↲↰",
                FilePath = @"C:\temp\",
                HashValue = 893476,
                MetadataName = "KeyValuePair"
            };
        }

        [Test]
        public void Should_be_able_to_send_and_receive_messages_by_interface_type_and_destination_name()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);
            messaging.SendMessage(testMessage);

            var finalObject = (IMetadataFile)messaging.GetMessage<IMsg>("Test_Destination");

            Assert.That(finalObject, Is.Not.Null);
            Assert.That(finalObject.CorrelationId, Is.EqualTo(testMessage.CorrelationId));
            Assert.That(finalObject.Contents, Is.EqualTo(testMessage.Contents));
            Assert.That(finalObject.FilePath, Is.EqualTo(testMessage.FilePath));
            Assert.That(finalObject.HashValue, Is.EqualTo(testMessage.HashValue));
            Assert.That(finalObject.MetadataName, Is.EqualTo(testMessage.MetadataName));
            Assert.That(finalObject.Equals(testMessage), Is.False);
        }

        [Test]
        public void can_handle_very_long_contract_names ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);
            IPreparedMessage prepMsg = new PreparedMessage("Example.Types.IMsg", "{}",
                @"Non.eram.nescius;Brute;cum;quae.summis.ingeniis.exquisitaque.doctrina.philosophi.Graeco.sermone.tractavissent;ea.Latinis.litteris.mandaremus;fore.ut.hic.noster.labor.in.varias.reprehensiones.incurreret.nam.quibusdam;et.iis.quidem.non.admodum.indoctis;totum.hoc.displicet.philosophari.quidam.autem.non.tam.id.reprehendunt;si.remissius.agatur;sed.tantum.studium.tamque.multam.operam.ponendam.in.eo.non.arbitrantur.erunt.etiam;et.ii.quidem.eruditi.Graecis.litteris;contemnentes.Latinas;qui.se.dicant.in.Graecis.legendis.operam.malle.consumere.postremo.aliquos.futuros.suspicor;qui.me.ad.alias.litteras.vocent;genus.hoc.scribendi;etsi.sit.elegans;personae.tamen.et.dignitatis.esse.negent.Contra.quos.omnis.dicendum.breviter.existimo.Quamquam.philosophiae.quidem.vituperatoribus.satis.responsum.est.eo.libro;quo.a.nobis.philosophia.defensa.et.collaudata.est;cum.esset.accusata.et.vituperata.ab.Hortensio.qui.liber.cum.et.tibi.probatus.videretur.et.iis;quos.ego.posse.iudicare.arbitrarer;plura.suscepi.veritus.ne.movere.hominum.studia.viderer;retinere.non.posse.Qui.autem;si.maxime.hoc.placeat;moderatius.tamen.id.volunt.fieri;difficilem.quandam.temperantiam.postulant.in.eo;quod.semel.admissum.coerceri.reprimique.non.potest;ut.propemodum.iustioribus.utamur.illis;qui.omnino.avocent.a.philosophia;quam.his;qui.rebus.infinitis.modum.constituant.in.reque.eo.meliore;quo.maior.sit;mediocritatem.desiderent.Sive.enim.ad.sapientiam.perveniri.potest;non.paranda.nobis.solum.ea;sed.fruenda.etiam.[sapientia].est;sive.hoc.difficile.est;tamen.nec.modus.est.ullus.investigandi.veri;nisi.inveneris;et.quaerendi.defatigatio.turpis.est;cum.id;quod.quaeritur;sit.pulcherrimum.etenim.si.delectamur;cum.scribimus;quis.est.tam.invidus;qui.ab.eo.nos.abducat?.sin.laboramus;quis.est;qui.alienae.modum.statuat.industriae?.nam.ut.Terentianus.Chremes.non.inhumanus;qui.novum.vicinum.non.vult.fodere.aut.arare.aut.aliquid.ferre.denique..non.enim.illum.ab.industria;sed.ab.inliberali.labore.deterret;sic.isti.curiosi;quos.offendit.noster.minime.nobis.iniucundus.labor;Example.Types.IMsg");
            messaging.SendPrepared(prepMsg);

            var roundTrip = messaging.TryStartMessageRaw("Test_Destination");
            Assert.That(roundTrip, Is.Not.Null, "Message did not arrive");

            Assert.That(roundTrip.Properties.OriginalType, Is.EqualTo(prepMsg.ContractType), "Contract changed in flight");
        }

        [Test]
        public void type_headers_survive_a_send_and_receive_round_trip ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);
            messaging.SendMessage(testMessage);

            var message = messaging.TryStartMessage<IMsg>("Test_Destination");

            Assert.That(message, Is.Not.Null);
            Console.WriteLine(message.Properties.OriginalType);
            Assert.That(message.Properties.OriginalType, Is.EqualTo(
                "Example.Types.IMetadataFile;" +
                "Example.Types.IFile;" +
                "Example.Types.IHash;" +
                "Example.Types.IPath;" +
                "Example.Types.IMsg"));
        }
		
        [Test]
        public void messages_can_be_picked_up_from_application_group_name_queue ()
        {
            messaging.CreateDestination<IMsg>("app-group-name", Expires.Never);
            messaging.SendMessage(testMessage);

            var message = messaging.TryStartMessage();

            Assert.That(message, Is.Not.Null);
            Console.WriteLine(message.Properties.OriginalType);
            Assert.That(message.Message is IMetadataFile, Is.True, "Lost type information");
        }

        [Test]
        public void configured_sender_name_is_returned_with_message ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);
            messaging.SendMessage(testMessage);

            var message = messaging.TryStartMessage<IMsg>("Test_Destination");

            Assert.That(message, Is.Not.Null);
            Console.WriteLine(message.Properties.OriginalType);
            Assert.That(message.Properties.SenderName, Is.EqualTo("app-group-name"));
        }

        [Test]
        public void Should_be_able_to_send_and_receive_messages_using_prepare_message_intermediates()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);
            byte[] raw = messaging.PrepareForSend(testMessage).ToBytes();

            messaging.SendPrepared(PreparedMessage.FromBytes(raw));

            var finalObject = (IMetadataFile)messaging.GetMessage<IMsg>("Test_Destination");

            Assert.That(finalObject, Is.Not.Null);
            Assert.That(finalObject.CorrelationId, Is.EqualTo(testMessage.CorrelationId));
            Assert.That(finalObject.Contents, Is.EqualTo(testMessage.Contents));
            Assert.That(finalObject.FilePath, Is.EqualTo(testMessage.FilePath));
            Assert.That(finalObject.HashValue, Is.EqualTo(testMessage.HashValue));
            Assert.That(finalObject.MetadataName, Is.EqualTo(testMessage.MetadataName));
            Assert.That(finalObject.Equals(testMessage), Is.False);
        }

        [Test]
        public void Should_be_able_to_send_and_receive_messages_by_destination_name_and_get_correct_type()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);
            messaging.SendMessage(testMessage);

            var finalObject = (IMetadataFile)messaging.GetMessage<IMsg>("Test_Destination");

            Assert.That(finalObject, Is.Not.Null);
            Assert.That(finalObject.CorrelationId, Is.EqualTo(testMessage.CorrelationId));
            Assert.That(finalObject.Contents, Is.EqualTo(testMessage.Contents));
            Assert.That(finalObject.FilePath, Is.EqualTo(testMessage.FilePath));
            Assert.That(finalObject.HashValue, Is.EqualTo(testMessage.HashValue));
            Assert.That(finalObject.MetadataName, Is.EqualTo(testMessage.MetadataName));
            Assert.That(finalObject.Equals(testMessage), Is.False);
        }

        [Test]
        public void should_be_able_to_get_cancel_get_again_and_finish_messages()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);
            messaging.SendMessage(testMessage);

            var pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
            var pending_2 = messaging.TryStartMessage<IMsg>("Test_Destination");
            Assert.That(pending_1, Is.Not.Null);
            Assert.That(pending_2, Is.Null);

            pending_1.Cancel();
            pending_2 = messaging.TryStartMessage<IMsg>("Test_Destination");
            Assert.That(pending_2, Is.Not.Null);

            pending_2.Finish();
            var pending_3 = messaging.TryStartMessage<IMsg>("Test_Destination");
            Assert.That(pending_3, Is.Null);

            var finalObject = (IMetadataFile)pending_2.Message;
            Assert.That(finalObject.CorrelationId, Is.EqualTo(testMessage.CorrelationId));
            Assert.That(finalObject.Contents, Is.EqualTo(testMessage.Contents));
            Assert.That(finalObject.FilePath, Is.EqualTo(testMessage.FilePath));
            Assert.That(finalObject.HashValue, Is.EqualTo(testMessage.HashValue));
            Assert.That(finalObject.MetadataName, Is.EqualTo(testMessage.MetadataName));
            Assert.That(finalObject.Equals(testMessage), Is.False);
        }

        [Test]
        public void should_protect_from_cancelling_the_same_message_twice ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);
            messaging.SendMessage(testMessage);

            var pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
            Assert.That(pending_1, Is.Not.Null);

            pending_1.Cancel();
            Assert.Throws<InvalidOperationException>(() => pending_1.Cancel());

            pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
            Assert.That(pending_1, Is.Not.Null);
            pending_1.Finish();

            Assert.Pass();
        }

        [Test]
        public void should_protect_from_finishing_the_same_message_twice ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);
            messaging.SendMessage(testMessage);

            var pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
            Assert.That(pending_1, Is.Not.Null);

            pending_1.Finish();
            Assert.Throws<InvalidOperationException>(() => pending_1.Finish());

            messaging.SendMessage(testMessage);

            pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
            Assert.That(pending_1, Is.Not.Null);
            pending_1.Finish();

            Assert.Pass();
        }

        [Test]
        public void should_protect_from_cancelling_then_finishing_a_message ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);
            messaging.SendMessage(testMessage);

            var pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
            Assert.That(pending_1, Is.Not.Null);

            pending_1.Cancel();
            Assert.Throws<InvalidOperationException>(() => pending_1.Finish());

            pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
            Assert.That(pending_1, Is.Not.Null);
            pending_1.Finish();

            Assert.Pass();
        }

        [Test]
        public void should_protect_from_finishing_then_cancelling_a_message ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);
            messaging.SendMessage(testMessage);

            var pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
            Assert.That(pending_1, Is.Not.Null);

            pending_1.Finish();
            Assert.Throws<InvalidOperationException>(() => pending_1.Cancel());

            messaging.SendMessage(testMessage);

            pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
            Assert.That(pending_1, Is.Not.Null);
            pending_1.Finish();

            Assert.Pass();
        }

        [Test]
        public void Should_be_able_to_send_and_receive_1000_messages_in_a_minute()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);

            int sent = 1000;
            int received = 0;
            var start = DateTime.Now;
            for (int i = 0; i < sent; i++)
            {
                messaging.SendMessage(testMessage);
            }

            Console.WriteLine("Sending took " + ((DateTime.Now) - start));
            var startGet = DateTime.Now;

            while (messaging.GetMessage<IMsg>("Test_Destination") != null)
            {
                Interlocked.Increment(ref received);
            }
            Console.WriteLine("Receiving took " + ((DateTime.Now) - startGet));

            var time = (DateTime.Now) - start;
            Assert.That(received, Is.EqualTo(sent));
            Assert.That(time.TotalSeconds, Is.LessThanOrEqualTo(60));
        }

        [OneTimeTearDown]
        public void cleanup()
        {
            MessagingBaseConfiguration.LastConfiguration.Get<IMessageRouter>().RemoveRouting(n=>true);
        }
    }
}