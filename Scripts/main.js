
document.addEventListener('DOMContentLoaded', () => {
    console.log('HSV Housing Website Loaded!');

    // --- Global Header/Navigation Logic ---
    const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
    if (navLinks) {
        navLinks.forEach(link => {
            if (link.href === window.location.href) {
                link.classList.add('active'); // Highlight active nav item
            }
        });
    }

    // --- Image Gallery Logic (for listing-detail.html) ---
    const mainImage = document.getElementById('mainListingImage');
    const thumbnailsContainer = document.getElementById('listingThumbnails');

    if (mainImage && thumbnailsContainer) {
        const thumbnails = thumbnailsContainer.querySelectorAll('.detail-thumbnail-item');

        thumbnails.forEach(thumbnail => {
            thumbnail.addEventListener('click', () => {
                // Remove active class from all thumbnails
                thumbnails.forEach(t => t.classList.remove('active'));
                // Add active class to clicked thumbnail
                thumbnail.classList.add('active');
                // Change main image source
                mainImage.src = thumbnail.src;
            });
        });

        // Set the first thumbnail as active by default
        if (thumbnails.length > 0) {
            thumbnails[0].classList.add('active');
        }
    }


    // --- Form Image Upload Preview (for create-listing.html) ---
    const imageUploadInput = document.getElementById('imageUploadInput');
    const imagePreviewArea = document.getElementById('imagePreviewArea');

    if (imageUploadInput && imagePreviewArea) {
        imageUploadInput.addEventListener('change', (event) => {
            imagePreviewArea.innerHTML = ''; // Clear previous previews

            const files = event.target.files;
            if (files.length === 0) {
                return;
            }

            Array.from(files).forEach(file => {
                const reader = new FileReader();
                reader.onload = (e) => {
                    const previewItem = document.createElement('div');
                    previewItem.classList.add('uploaded-image-item');
                    previewItem.innerHTML = `
                        <img src="${e.target.result}" alt="Preview">
                        <button type="button" class="remove-image-btn">&times;</button>
                    `;
                    imagePreviewArea.appendChild(previewItem);

                    // Add remove functionality
                    previewItem.querySelector('.remove-image-btn').addEventListener('click', () => {
                        previewItem.remove();
                        // TODO: Implement logic to remove file from input if needed
                    });
                };
                reader.readAsDataURL(file);
            });
        });
    }

    // --- Dynamic Ward/District Dropdowns (example for create-listing.html) ---
    const districtSelect = document.getElementById('districtSelect');
    const wardSelect = document.getElementById('wardSelect');

    if (districtSelect && wardSelect) {
        const wardsData = {
            '1': ['Phường A', 'Phường B', 'Phường C'], // Wards for District 1
            '2': ['Phường X', 'Phường Y'],             // Wards for District 2
            // ... more data
        };

        const updateWards = () => {
            const selectedDistrictId = districtSelect.value;
            wardSelect.innerHTML = '<option value="">Chọn Phường/Xã</option>'; // Reset wards

            if (selectedDistrictId && wardsData[selectedDistrictId]) {
                wardsData[selectedDistrictId].forEach(wardName => {
                    const option = document.createElement('option');
                    option.value = wardName; // Or ward ID from backend
                    option.textContent = wardName;
                    wardSelect.appendChild(option);
                });
            }
        };

        districtSelect.addEventListener('change', updateWards);
        updateWards(); // Call initially to populate wards based on default district
    }
});