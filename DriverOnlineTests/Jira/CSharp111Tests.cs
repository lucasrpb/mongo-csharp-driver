﻿/* Copyright 2010 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

using MongoDB.Bson;
using MongoDB.Bson.DefaultSerializer;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MongoDB.DriverOnlineTests.Jira.CSharp111 {
    [TestFixture]
    public class CSharp111Tests {
        private class C {
            public ObjectId Id;
            public List<D> InnerObjects;
        }

        private class D {
            public int X;
        }

        [Test]
        public void TestAddToSetEach() {
            var server = MongoServer.Create("mongodb://localhost/?safe=true");
            var database = server["onlinetests"];
            var collection = database.GetCollection<C>("csharp111");

            collection.RemoveAll();
            var c = new C { InnerObjects = new List<D>() };
            collection.Insert(c);
            var id = c.Id;

            var innerObjects = new List<D> { new D { X = 1 }, new D { X = 2 } };
            var innerBsonValues = innerObjects.ConvertAll(obj => obj.ToBsonDocument<D>()).ToArray();
            var query = Query.EQ("_id", id);
            var update = Update.AddToSetEach("InnerObjects", innerBsonValues);
            collection.Update(query, update);

            var document = collection.FindOneAs<BsonDocument>();
            var json = document.ToJson();
            var expected = "{ 'InnerObjects' : [{ 'X' : 1 }, { 'X' : 2 }], '_id' : { '$oid' : '#ID' } }"; // server put _id at end?
            expected = expected.Replace("#ID", id.ToString());
            expected = expected.Replace("'", "\"");
            Assert.AreEqual(expected, json);
        }
    }
}