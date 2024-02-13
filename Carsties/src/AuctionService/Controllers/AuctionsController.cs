using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/auctions")]
    [ApiController]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;

        public AuctionsController(AuctionDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
        {
            var auctionsList = await (from a in _context.Auctions
                                      from i in _context.Items
                                      select new AuctionDto()
                                      {
                                          Id = a.Id,
                                          ReservePrice = a.ReservePrice,
                                          Seller = a.Seller,
                                          Winner = a.Winner,
                                          SoldAmount = a.SoldAmount,
                                          CurrentHighBid = a.CurrentHighBid,
                                          AuctionEnd = a.AuctionEnd,
                                          Status = nameof(a.Status),
                                          Make = i.Make,
                                          Model = i.Model,
                                          Year = i.Year,
                                          Color = i.Color,
                                          Mileage = i.Mileage,
                                          ImageUrl = i.ImageUrl
                                      }).OrderBy(x => x.Make).ToListAsync();

            return auctionsList;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await (from a in _context.Auctions.Where(a => a.Id == id)
                                 from i in _context.Items
                                 select new AuctionDto()
                                 {
                                     Id = a.Id,
                                     ReservePrice = a.ReservePrice,
                                     Seller = a.Seller,
                                     Winner = a.Winner,
                                     SoldAmount = a.SoldAmount,
                                     CurrentHighBid = a.CurrentHighBid,
                                     AuctionEnd = a.AuctionEnd,
                                     Status = nameof(a.Status),
                                     Make = i.Make,
                                     Model = i.Model,
                                     Year = i.Year,
                                     Color = i.Color,
                                     Mileage = i.Mileage,
                                     ImageUrl = i.ImageUrl
                                 }).FirstOrDefaultAsync();

            if (auction == null) return NotFound();

            return auction;
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = new Auction()
            {
                ReservePrice = auctionDto.ReservePrice,
                AuctionEnd = auctionDto.AuctionEnd,
                Seller = "test" // TODO: add current user as seller
            };
            
            _context.Auctions.Add(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                var item = new Item()
                {
                    Make = auctionDto.Make,
                    Model = auctionDto.Model,
                    Year = auctionDto.Year,
                    Color = auctionDto.Color,
                    Mileage = auctionDto.Mileage,
                    ImageUrl = auctionDto.ImageUrl,
                    AuctionId = auction.Id
                };

                _context.Items.Add(item);

                result = await _context.SaveChangesAsync() > 0;
            }

            var auctionResponseDto = new AuctionDto()
            {
                ReservePrice = auctionDto.ReservePrice,
                AuctionEnd = auctionDto.AuctionEnd,
                Seller = auction.Seller,
                Make = auctionDto.Make,
                Model = auctionDto.Model,
                Year = auctionDto.Year,
                Color = auctionDto.Color,
                Mileage = auctionDto.Mileage,
                ImageUrl = auctionDto.ImageUrl,
                Id = auction.Id
            };

            if (!result) return BadRequest("Could not save changes to the DB");

            return CreatedAtAction(nameof(GetAuctionById),
                new { auction.Id }, auctionResponseDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            // TODO: check seller == username

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Problem saving changes");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction == null) return NotFound();

            // TODO: check seller == username

            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not update DB");

            return Ok();
        }
    }
}
