﻿using AutoMapper;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Helper.Extensions;

namespace ShareBook.Api.AutoMapper
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile() : this("Profile")
        {
        }

        protected DomainToViewModelMappingProfile(string profileName) : base(profileName)
        {
            #region [ Book ]

            CreateMap<Book, BookVMAdm>()
                 .ForMember(dest => dest.Donor, opt => opt.MapFrom(src => src.User.Name))
                 .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.User.Address.City))
                 .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.User.Address.State))
                 .ForMember(dest => dest.Facilitator, opt => opt.MapFrom(src => src.UserFacilitator.Name))
                 .ForMember(dest => dest.FacilitatorNotes, opt => opt.MapFrom(src => src.FacilitatorNotes))
                 .ForMember(dest => dest.PhoneDonor, opt => opt.MapFrom(src => src.User.Phone))
                 .ForMember(dest => dest.DaysInShowcase, opt => opt.MapFrom(src => src.DaysInShowcase()))
                 .ForMember(dest => dest.TotalInterested, opt => opt.MapFrom(src => src.TotalInterested()))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                 .ForMember(dest => dest.FreightOption, opt => opt.MapFrom(src => src.FreightOption.ToString()))
                 .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationDate))
                 .ForMember(dest => dest.ChooseDate, opt => opt.MapFrom(src => src.ChooseDate))
                 .ForMember(dest => dest.Winner, opt => opt.MapFrom(src => src.WinnerName()))
                 .ForMember(dest => dest.TrackingNumber, opt => opt.MapFrom(src => src.TrackingNumber))
                 .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<Book, BookVM>()
                 .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.User.Address.City))
                 .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.User.Address.State))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                 .ForMember(dest => dest.FreightOption, opt => opt.MapFrom(src => src.FreightOption.ToString()))
                 .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationDate))
                 .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<BookRequest, MyBookRequestVM>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.BookRequested.Author))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.BookRequested.Title))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.BookStatus, opt => opt.MapFrom(src => src.BookRequested.Status.ToString()))
                .ForMember(dest => dest.TrackingNumber, opt => opt.MapFrom(src => src.BookRequested.TrackingNumber))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.BookRequested.Slug));

            #endregion [ Book ]

            #region [ User ]

            CreateMap<User, UserVM>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));

            CreateMap<User, UserFacilitatorVM>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            #endregion [ User ]

            #region [ BookUser ]

            CreateMap<BookRequest, RequestersListVM>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.RequestUserId))
                .ForMember(dest => dest.RequesterNickName, opt => opt.MapFrom(src => src.NickName))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.DonorUser.Location()))
                .ForMember(dest => dest.TotalBooksWon, opt => opt.MapFrom(src => src.DonorUser.TotalBooksWon()))
                .ForMember(dest => dest.TotalBooksDonated, opt => opt.MapFrom(src => src.DonorUser.TotalBooksDonated()))
                .ForMember(dest => dest.RequestText, opt => opt.MapFrom(src => src.Reason))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            #endregion [ BookUser ]
        }
    }
}