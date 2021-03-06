﻿namespace King.ATrak.Test.Azure
{
    using King.ATrak.Azure;
    using King.Azure.Data;
    using Microsoft.WindowsAzure.Storage.Blob;
    using NSubstitute;
    using NUnit.Framework;
    using System;
    using System.Threading.Tasks;

    [TestFixture]
    public class BlobItemTests
    {
        [Test]
        public void Constructor()
        {
            var container = Substitute.For<IContainer>();
            new BlobItem(container, "/file.txt");
        }

        [Test]
        public void ConstructorContainerNull()
        {
            Assert.That(() => new BlobItem(null, "/file.txt"), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ConstructorPathNull()
        {
            var container = Substitute.For<IContainer>();

            Assert.That(() => new BlobItem(container, null), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void IsIStorageItem()
        {
            var container = Substitute.For<IContainer>();
            Assert.IsNotNull(new BlobItem(container, "/file.txt") as IStorageItem);
        }

        [Test]
        public void Path()
        {
            var container = Substitute.For<IContainer>();
            container.Name.Returns("container");

            var bi = new BlobItem(container, "/file.txt");

            Assert.AreEqual("container/file.txt", bi.Path);

            var z = container.Received().Name;
        }

        [Test]
        public void RelativePath()
        {
            var container = Substitute.For<IContainer>();

            var bi = new BlobItem(container, "/file.txt");

            Assert.AreEqual("file.txt", bi.RelativePath);
        }

        [Test]
        public void RelativePathNoForwardSlash()
        {
            var container = Substitute.For<IContainer>();

            var bi = new BlobItem(container, "file.txt");

            Assert.AreEqual("file.txt", bi.RelativePath);
        }

        [Test]
        public async Task Load()
        {
            var random = new Random();
            var bytes = new byte[64];
            random.NextBytes(bytes);
            var p = new BlobProperties()
            {
                ContentType = Guid.NewGuid().ToString(),
                ContentMD5 = Guid.NewGuid().ToString(),
            };

            var container = Substitute.For<IContainer>();
            container.Get("file.txt").Returns(Task.FromResult(bytes));

            var bi = new BlobItem(container, "/file.txt");
            await bi.Load();

            Assert.AreEqual(bytes, bi.Data);

            container.Received().Get("file.txt");
        }

        [Test]
        public async Task LoadMD5()
        {
            var random = new Random();
            var bytes = new byte[64];
            random.NextBytes(bytes);
            var p = new BlobProperties()
            {
                ContentType = Guid.NewGuid().ToString(),
                ContentMD5 = Guid.NewGuid().ToString(),
            };

            var container = Substitute.For<IContainer>();
            container.Properties("file.txt").Returns(Task.FromResult(p));

            var bi = new BlobItem(container, "/file.txt");
            await bi.LoadMD5();

            Assert.AreEqual(p.ContentType, bi.ContentType);
            Assert.AreEqual(p.ContentMD5, bi.MD5);

            container.Received().Properties("file.txt");
        }

        [Test]
        public async Task Delete()
        {
            var container = Substitute.For<IContainer>();
            container.Delete("file.txt");

            var bi = new BlobItem(container, "/file.txt");
            await bi.Delete();

            container.Received().Delete("file.txt");
        }
    }
}