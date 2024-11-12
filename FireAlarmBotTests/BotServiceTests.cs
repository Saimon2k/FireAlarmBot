using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace Tests
{
    public class BotServiceTests
    {
        private readonly Mock<ILogger<BotService>> _loggerMock;
        private readonly Mock<ITelegramBotClient> _botClientMock;
        private readonly Mock<FloorService> _floorServiceMock;
        private readonly BotService _botService;

        public BotServiceTests()
        {
            _loggerMock = new Mock<ILogger<BotService>>();
            _botClientMock = new Mock<ITelegramBotClient>();
            _floorServiceMock = new Mock<FloorService>();
            _botService = new BotService(_loggerMock.Object, _botClientMock.Object, _floorServiceMock.Object);
        }

        [Fact]
        public async Task HandleUpdateAsync_ShouldAddSingleFloor()
        {
            // Arrange
            var update = new Update
            {
                Message = new Message
                {
                    Text = "/check 5",
                    Chat = new Chat { Id = 1 },
                    From = new User { Id = 123 },
                    Type = MessageType.Text
                }
            };

            // Act
            await _botService.HandleUpdateAsync(_botClientMock.Object, update, CancellationToken.None);

            // Assert
            _floorServiceMock.Verify(f => f.AddFloor(5), Times.Once);
            _botClientMock.Verify(b => b.SendTextMessageAsync(1, "Этаж 5 добавлен", null, false, false, 0, false, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task HandleUpdateAsync_ShouldAddFloorRange()
        {
            // Arrange
            var update = new Update
            {
                Message = new Message
                {
                    Text = "/check 1-3",
                    Chat = new Chat { Id = 1 },
                    From = new User { Id = 123 },
                    Type = MessageType.Text
                }
            };

            // Act
            await _botService.HandleUpdateAsync(_botClientMock.Object, update, CancellationToken.None);

            // Assert
            _floorServiceMock.Verify(f => f.AddFloor(1), Times.Once);
            _floorServiceMock.Verify(f => f.AddFloor(2), Times.Once);
            _floorServiceMock.Verify(f => f.AddFloor(3), Times.Once);
            _botClientMock.Verify(b => b.SendTextMessageAsync(1, "Этажи с 1 по 3 добавлены.", null, false, false, 0, false, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task HandleUpdateAsync_ShouldAddMultipleFloors()
        {
            // Arrange
            var update = new Update
            {
                Message = new Message
                {
                    Text = "/check 1, 2, 3",
                    Chat = new Chat { Id = 1 },
                    From = new User { Id = 123 },
                    Type = MessageType.Text
                }
            };

            // Act
            await _botService.HandleUpdateAsync(_botClientMock.Object, update, CancellationToken.None);

            // Assert
            _floorServiceMock.Verify(f => f.AddFloor(1), Times.Once);
            _floorServiceMock.Verify(f => f.AddFloor(2), Times.Once);
            _floorServiceMock.Verify(f => f.AddFloor(3), Times.Once);
            _botClientMock.Verify(b => b.SendTextMessageAsync(1, "Этажи 1, 2, 3 добавлены", null, false, false, 0, false, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task HandleUpdateAsync_ShouldReturnSummary()
        {
            // Arrange
            var update = new Update
            {
                Message = new Message
                {
                    Text = "/summary",
                    Chat = new Chat { Id = 1 },
                    From = new User { Id = 123 },
                    Type = MessageType.Text
                }
            };

            _floorServiceMock.Setup(f => f.GetUniqueFloors()).Returns(new[] { 1, 2, 3 });
            _floorServiceMock.Setup(f => f.GetUncheckedFloors()).Returns(new[] { 4, 5 });

            // Act
            await _botService.HandleUpdateAsync(_botClientMock.Object, update, CancellationToken.None);

            // Assert
            var expectedResponse = "Записанные этажи: 1, 2, 3\nНепроверенные этажи: 4, 5";
            _botClientMock.Verify(b => b.SendTextMessageAsync(1, expectedResponse, null, false, false, 0, false, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task HandleUpdateAsync_ShouldReturnJoke()
        {
            // Arrange
            var update = new Update
            {
                Message = new Message
                {
                    Text = "/joke",
                    Chat = new Chat { Id = 1 },
                    From = new User { Id = 123 },
                    Type = MessageType.Text
                }
            };

            _floorServiceMock.Setup(f => f.GetUniqueFloors()).Returns(new[] { 1, 2, 3 });
            _floorServiceMock.Setup(f => f.GetUncheckedFloors()).Returns(new[] { 4, 5 });
            _floorServiceMock.Setup(f => f.GetTotalFloorsChecked()).Returns(10);
            _floorServiceMock.Setup(f => f.GetUniqueFloorCount()).Returns(3);
            _floorServiceMock.Setup(f => f.GetMostCheckedFloor()).Returns(2);
            _floorServiceMock.Setup(f => f.GetLastCheckedFloor()).Returns(3);
            _floorServiceMock.Setup(f => f.GetCheckedPercentage()).Returns(75.0);

            // Act
            await _botService.HandleUpdateAsync(_botClientMock.Object, update, CancellationToken.None);

            // Assert
            var expectedResponse = "Записанные этажи: 1, 2, 3\n" +
                                   "Непроверенные этажи: 4, 5\n" +
                                   "Всего проверок: 10\n" +
                                   "Уникальных этажей: 3\n" +
                                   "Самый часто проверяемый этаж: 2\n" +
                                   "Последний проверенный этаж: 3\n" +
                                   "Процент проверенных этажей: 75.00%";
            _botClientMock.Verify(b => b.SendTextMessageAsync(1, expectedResponse, null, false, false, 0, false, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task HandleUpdateAsync_ShouldHandleInvalidFloorRange()
        {
            // Arrange
            var update = new Update
            {
                Message = new Message
                {
                    Text = "/check 1-a",
                    Chat = new Chat { Id = 1 },
                    From = new User { Id = 123 },
                    Type = MessageType.Text
                }
            };

            // Act
            await _botService.HandleUpdateAsync(_botClientMock.Object, update, CancellationToken.None);

            // Assert
            _botClientMock.Verify(b => b.SendTextMessageAsync(1, "Некорректный диапазон этажей. Использование: /check 1-10", null, false, false, 0, false, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task HandleUpdateAsync_ShouldHandleInvalidFloorNumber()
        {
            // Arrange
            var update = new Update
            {
                Message = new Message
                {
                    Text = "/check abc",
                    Chat = new Chat { Id = 1 },
                    From = new User { Id = 123 },
                    Type = MessageType.Text
                }
            };

            // Act
            await _botService.HandleUpdateAsync(_botClientMock.Object, update, CancellationToken.None);

            // Assert
            _botClientMock.Verify(b => b.SendTextMessageAsync(1, "Некорректный номер этажа", null, false, false, 0, false, CancellationToken.None), Times.Once);
        }
    }
}