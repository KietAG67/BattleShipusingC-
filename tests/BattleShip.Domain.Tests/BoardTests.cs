using BattleShip.Domain;
using Xunit;
namespace BattleShip.Domain.Tests;
public class BoardTests { [Fact] public void CannotOverlap(){var b=new Board();b.PlaceShip("A",3,new(0,0),Orientation.Horizontal);Assert.False(b.CanPlaceShip(2,new(0,2),Orientation.Vertical));} [Fact] public void CannotLeaveBoard(){var b=new Board();Assert.False(b.CanPlaceShip(5,new(9,8),Orientation.Horizontal));} [Fact] public void DuplicateShotRejected(){var b=new Board();b.PlaceShip("A",2,new(0,0),Orientation.Horizontal);Assert.Equal(ShotResult.Hit,b.Shoot(new(0,0)));Assert.Equal(ShotResult.AlreadyShot,b.Shoot(new(0,0)));} }
