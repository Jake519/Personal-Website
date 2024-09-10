window.onscroll = function() {
    if (window.scrollInfoService != null) {
        scrollValue = parseInt(window.scrollY);
        window.scrollInfoService.invokeMethodAsync('OnScroll', scrollValue);
    }
}

window.RegisterScrollInfoService = (scrollInfoService) => {
    window.scrollInfoService = scrollInfoService;
}