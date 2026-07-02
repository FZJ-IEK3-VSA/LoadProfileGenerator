using Automation;
using Automation.ResultFiles;
using Common;
using Common.Tests;
using Xunit;
using Xunit.Abstractions;
#pragma warning disable 8602
namespace SimulationEngine.Tests
{
    public class SystematicTransportHouseholdTests : UnitTestBaseClass
    {
        private static HouseCreationAndCalculationJob MakeHouseJob(Database.Simulator sim, string hhguid)
        {
            var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid, TestDuration.ThreeMonths);
            if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
            hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
            return hj;
        }

        private static void RunHouseJobNoChecks(string hhguid)
        {
            string testname = Utili.GetCallingMethodAndClass();
            HouseJobTestHelper.RunSingleHouse(sim => MakeHouseJob(sim, hhguid), _ => { }, testname: testname);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport0()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport1()
        {
            const string hhguid = "1a7c45dc-272a-4836-bca9-076bd200486a";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport2()
        {
            const string hhguid = "e41a31b5-8eb1-4ec1-8875-49d0d4441f33";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport3()
        {
            const string hhguid = "5da74745-b625-4311-8f69-6ef3351207c5";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport4()
        {
            const string hhguid = "f0c151a4-ee8d-4a23-9cd1-6858d258aef8";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport5()
        {
            const string hhguid = "c1248c1a-a654-486c-8e20-2435dc0cad4d";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport6()
        {
            const string hhguid = "20173a11-f1ac-44ef-952d-4c5a65ac3988";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport7()
        {
            const string hhguid = "e30d5760-b89d-4087-ac5a-c33b3250b000";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport8()
        {
            const string hhguid = "f6309e9c-af83-44e8-9381-12766e6dc8a4";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport9()
        {
            const string hhguid = "2b85a956-a211-4b39-9c66-41144394a3fe";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport10()
        {
            const string hhguid = "57b0bafd-93ce-4ae1-a0ec-568eb41e3a88";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport11()
        {
            const string hhguid = "d4fb5502-660e-4d1e-bc9f-ca07dc4882ef";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport12()
        {
            const string hhguid = "f2a97869-7a3d-4efc-8565-51b3c43ba183";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport13()
        {
            const string hhguid = "65bd2299-3174-4531-b4fd-fc327b6fc3f6";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport14()
        {
            const string hhguid = "f1470a33-c934-4203-b7cb-184b6dc07633";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport15()
        {
            const string hhguid = "8260de8b-2fa6-4a36-bf40-5304afb2fc1a";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport16()
        {
            const string hhguid = "61668b2d-0559-4dd2-815d-9d2725222690";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport17()
        {
            const string hhguid = "0d17c119-8566-4eac-b610-33bd6f764878";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport18()
        {
            const string hhguid = "919ccda6-7a07-49e3-a4b0-bbba2410c70e";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport19()
        {
            const string hhguid = "68edfea5-f8d4-4a6f-a2ae-313ee2e41624";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport20()
        {
            const string hhguid = "fd1406f4-1f65-43ba-9504-9425f6eb01ef";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport21()
        {
            const string hhguid = "d97ae616-e1ba-468a-85a0-627b8cc5e1cd";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport22()
        {
            const string hhguid = "92f23b58-d357-403f-ad30-f7ae63576893";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport23()
        {
            const string hhguid = "df908a28-6d5b-4d90-8a16-d0442c1c32e1";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport24()
        {
            const string hhguid = "a4e53285-125a-4eed-b37a-268f081ae444";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport25()
        {
            const string hhguid = "b8bdef97-556a-447d-8d46-2deda2516057";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport26()
        {
            const string hhguid = "dc267b29-cfec-476a-9399-2014058f36f6";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport27()
        {
            const string hhguid = "b833ceb3-5a19-419f-9835-b75b52f8be7c";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport28()
        {
            const string hhguid = "e3a959e4-562a-4b15-a820-6159e2b2dddc";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport29()
        {
            const string hhguid = "4fb7efde-3cef-4eb2-8ebe-e89f3ac87aed";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport30()
        {
            const string hhguid = "fee0cdc2-22f7-45c4-bf01-3aaf65866773";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport31()
        {
            const string hhguid = "0dad3b57-f255-4c9c-9096-eef45ca3199c";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport32()
        {
            const string hhguid = "5220db46-4d23-410f-af0b-ab11ad1279bc";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport33()
        {
            const string hhguid = "25ce714a-9f93-4f8e-ba03-37b76a0294da";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport34()
        {
            const string hhguid = "3368c3e9-60f2-49e4-b79c-b1febc74485b";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport35()
        {
            const string hhguid = "d17d88c9-666d-4d24-aac1-78bef65c53a1";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport36()
        {
            const string hhguid = "86a5cf7a-e9c7-4f59-8a6c-f9cfe2b7fe03";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport37()
        {
            const string hhguid = "2335b994-d7fa-41c1-af93-0c401d192122";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport38()
        {
            const string hhguid = "afc3244b-2988-4f65-8c73-f4fcc1f531d2";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport39()
        {
            const string hhguid = "7cf13644-b837-4d93-9a90-0e32a295e4a9";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport40()
        {
            const string hhguid = "e5355495-afd3-490f-9dd0-3839d1f7f1d0";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport41()
        {
            const string hhguid = "23fb7efd-abcf-4caa-8434-bb6cfc87fdaf";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport42()
        {
            const string hhguid = "130aedcf-e0cc-4335-a6c8-594189fffefb";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport43()
        {
            const string hhguid = "c2ea56a1-6413-4bc3-9b0a-d0d5705434e1";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport44()
        {
            const string hhguid = "bab73822-78ce-4a3a-9164-3e0942fb6508";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport45()
        {
            const string hhguid = "442a31e8-ccb6-457a-8436-8c4d6acabc23";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport46()
        {
            const string hhguid = "820d9de7-4fc7-42af-bf7f-701a35675063";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport47()
        {
            const string hhguid = "567c3426-85dd-4ab5-917a-9f28f0cc9f76";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport48()
        {
            const string hhguid = "3c10c5de-b246-461a-b2bb-589ad80da159";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport49()
        {
            const string hhguid = "4c5fc522-9472-4b37-b7dc-1d72a24c2df1";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport50()
        {
            const string hhguid = "114871cb-345a-47c0-9138-6322367333d6";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport51()
        {
            const string hhguid = "debf4669-1be0-44a1-8010-30a7e8290559";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport52()
        {
            const string hhguid = "fe8adddd-8409-4f01-9ccc-f85dd018eff8";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport53()
        {
            const string hhguid = "b22ecb7c-4422-4e72-8af0-0f2c5d28441a";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport54()
        {
            const string hhguid = "b4451879-164c-4416-bd20-502fb471ccdc";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport55()
        {
            const string hhguid = "11195315-953b-46de-9572-ec7c10b2ce5e";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport56()
        {
            const string hhguid = "db51a7ef-16e9-49bc-8dec-1406a664d641";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport57()
        {
            const string hhguid = "747120ae-5203-4ed7-9bb5-b56a2075c5f5";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport58()
        {
            const string hhguid = "f497f10f-6628-4b34-8ce3-8daf8660e6a5";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport59()
        {
            const string hhguid = "e045f4b5-3086-4389-ba23-c026e40900c9";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport60()
        {
            const string hhguid = "e7cb1be5-caac-4087-83e8-c181911a68e2";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport61()
        {
            const string hhguid = "148a1c21-2a3a-49bf-93aa-20ac0e89724e";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport62()
        {
            const string hhguid = "1fd8d33c-97b2-4934-a681-d6b10446e462";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport63()
        {
            const string hhguid = "bc09654d-e1bf-4f66-b5f6-e97476455537";
            RunHouseJobNoChecks(hhguid);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]
        public void TestHouseholdWithTransport64()
        {
            const string hhguid = "65e73536-0d89-407c-bb47-00f67a9a0945";
            RunHouseJobNoChecks(hhguid);
        }

        public SystematicTransportHouseholdTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
    }
}
