﻿using BookToAudio.Infra.Services.Interfaces;
using Moq;

namespace BookToAudio.TestingInfra.Mocks;

public static class ITtsServiceFactoryMock
{
    public static Mock<ITtsServiceFactory> Get()
    {
        var ttsfactory = new Mock<ITtsServiceFactory>();

        ttsfactory.Setup(service =>
            service.Get(It.IsAny<string>()))
            .Returns(ITtsServiceMock.Get().Object);

        return ttsfactory;
    }
}
