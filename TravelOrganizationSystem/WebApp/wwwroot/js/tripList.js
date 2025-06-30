// Trip List AJAX functionality
// Handles pagination and filtering without page reloads

let currentPage = 1;
let currentDestinationId = null;
let pageSize = 10;
let isLoading = false;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    initializeTripList();
});

// Initialize AJAX functionality for trip list
function initializeTripList() {
    console.log('Initializing AJAX trip list functionality');
    
    // Get initial values from the page
    const urlParams = new URLSearchParams(window.location.search);
    currentPage = parseInt(urlParams.get('page')) || 1;
    currentDestinationId = urlParams.get('destinationId') || null;
    
    // Convert destination filter to AJAX
    const destinationSelect = document.getElementById('DestinationId');
    if (destinationSelect) {
        destinationSelect.addEventListener('change', function() {
            currentDestinationId = this.value || null;
            currentPage = 1; // Reset to first page when filtering
            loadTripsWithAjax();
        });
    }
    
    // Prevent form submission and use AJAX instead
    const filterForm = document.getElementById('tripFilterForm');
    if (filterForm) {
        filterForm.addEventListener('submit', function(e) {
            e.preventDefault();
            // The change event on select will already trigger AJAX call
            loadTripsWithAjax();
        });
    }
    
    // Convert pagination links to AJAX
    convertPaginationToAjax();
    
    // Add loading indicator if it doesn't exist
    addLoadingIndicator();
    
    // Initialize hover effects for existing cards
    initializeCardHoverEffects();
}

// Convert existing pagination links to use AJAX
function convertPaginationToAjax() {
    const paginationContainer = document.querySelector('.pagination');
    if (!paginationContainer) return;
    
    // Add event delegation for all pagination links
    paginationContainer.addEventListener('click', function(e) {
        e.preventDefault();
        
        const link = e.target.closest('a.page-link');
        if (!link) return;
        
        const href = link.getAttribute('href');
        if (!href) return;
        
        // Extract page number from href
        const urlParams = new URLSearchParams(href.split('?')[1] || '');
        const page = parseInt(urlParams.get('page'));
        
        if (page && page !== currentPage) {
            currentPage = page;
            loadTripsWithAjax();
        }
    });
}

// Load trips using AJAX
async function loadTripsWithAjax() {
    if (isLoading) return;
    
    try {
        isLoading = true;
        showLoadingIndicator();
        
        // Build query parameters
        const params = new URLSearchParams();
        params.append('page', currentPage.toString());
        params.append('pageSize', pageSize.toString());
        
        if (currentDestinationId) {
            params.append('destinationId', currentDestinationId);
        }
        
        console.log(`Loading trips: page ${currentPage}, destination ${currentDestinationId}`);
        
        // Make AJAX request
        const response = await fetch(`/api/trips?${params.toString()}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const data = await response.json();
        console.log('Received trip data:', data);
        
        // Update the page content
        updateTripList(data.trips);
        updatePagination(data.pagination);
        updateUrl();
        
        // Show success message
        showMessage('Trips loaded successfully', 'success');
        
    } catch (error) {
        console.error('Error loading trips:', error);
        showMessage(`Error loading trips: ${error.message}`, 'error');
    } finally {
        isLoading = false;
        hideLoadingIndicator();
    }
}

// Update the trip list content
function updateTripList(trips) {
    const tripContainer = document.querySelector('.row.row-cols-1.row-cols-md-3.g-4');
    if (!tripContainer) {
        console.error('Trip container not found');
        return;
    }
    
    // Clear existing trips (but keep the "Add New Trip" card if it exists)
    const addTripCard = tripContainer.querySelector('.card.border-primary');
    tripContainer.innerHTML = '';
    
    // Re-add the "Add New Trip" card if it existed
    if (addTripCard) {
        tripContainer.appendChild(addTripCard);
    }
    
    // Add new trips
    trips.forEach(trip => {
        const tripCard = createTripCard(trip);
        tripContainer.appendChild(tripCard);
    });
    
    // Reinitialize hover effects for newly created cards
    initializeCardHoverEffects();
    
    // Show message if no trips found
    if (trips.length === 0) {
        showNoTripsMessage();
    }
}

// Create a trip card element
function createTripCard(trip) {
    const col = document.createElement('div');
    col.className = 'col';
    
    // Format the dates
    const startDate = new Date(trip.startDate || trip.StartDate).toLocaleDateString('en-US', { 
        year: 'numeric', month: 'short', day: 'numeric' 
    });
    const endDate = new Date(trip.endDate || trip.EndDate).toLocaleDateString('en-US', { 
        year: 'numeric', month: 'short', day: 'numeric' 
    });
    
    // Format description with truncation
    const description = (trip.description || trip.Description || '') 
    const truncatedDescription = description.length > 150 
        ? description.substring(0, 150) + '...' 
        : description;
    
    // Determine availability
    const availableSlots = trip.availableSlots || trip.AvailableSlots || 0;
    const isAvailable = availableSlots > 0;
    const availabilityBadge = isAvailable 
        ? `<span class="badge bg-success float-end">Available</span>`
        : `<span class="badge bg-danger float-end">Sold Out</span>`;
    const availabilityText = isAvailable 
        ? `${availableSlots} spots left`
        : 'Fully booked';
    
    // Create image HTML with price badge
    const imageUrl = trip.imageUrl || trip.ImageUrl;
    const tripTitle = trip.title || trip.Title;
    const formattedPrice = trip.formattedPrice || trip.FormattedPrice || `$${trip.price || trip.Price}`;
    
    const imageHtml = imageUrl 
        ? `<img src="${imageUrl}" class="card-img-top" alt="${tripTitle}" style="height: 250px; object-fit: cover; border-radius: 20px 20px 0 0;">`
        : `<div class="card-img-top d-flex justify-content-center align-items-center" style="height: 250px; background: linear-gradient(135deg, rgba(52, 152, 219, 0.2), rgba(46, 204, 113, 0.2)); border-radius: 20px 20px 0 0;">
             <div class="text-center">
               <i class="fas fa-plane-departure fa-4x mb-2" style="color: #3498db; opacity: 0.7;"></i>
               <p class="mb-0" style="color: #bdc3c7;">No image available</p>
             </div>
           </div>`;
    
    const priceBadgeHtml = `
        <div class="position-absolute top-0 end-0 m-3">
            <span class="badge fs-6 px-3 py-2" style="background: rgba(0, 0, 0, 0.8); color: #2ecc71; backdrop-filter: blur(10px); border: 1px solid rgba(46, 204, 113, 0.3); box-shadow: 0 4px 15px rgba(0, 0, 0, 0.3); font-weight: 600;">
                <i class="fas fa-tag me-1"></i>${formattedPrice}
            </span>
        </div>`;
    
    // Create action buttons
    const isAuthenticated = document.body.getAttribute('data-user-authenticated') === 'true';
    const isAdmin = document.body.getAttribute('data-user-admin') === 'true';
    
        const tripId = trip.id || trip.Id;
    let actionButtons = `
        <a href="/Trips/Details/${tripId}" class="btn btn-sm btn-primary btn-animated" onclick="event.stopPropagation();">
            <i class="fas fa-info-circle me-1"></i> Details
        </a>`;
    
    if (isAuthenticated && isAvailable) {
        actionButtons += `
            <a href="/Trips/Book/${tripId}" class="btn btn-sm btn-success btn-animated" onclick="event.stopPropagation();">
                <i class="fas fa-ticket-alt me-1"></i> Book
            </a>`;
    }
    
    if (isAdmin) {
        actionButtons += `
            <div class="ms-auto d-flex gap-1">
                <a href="/Trips/Edit/${tripId}" class="btn btn-sm btn-outline-primary btn-animated" onclick="event.stopPropagation();">
                    <i class="fas fa-edit"></i>
                </a>
                <button type="button" class="btn btn-sm btn-outline-danger btn-animated" onclick="event.stopPropagation(); confirmDelete(${tripId}, '${tripTitle}');">
                    <i class="fas fa-trash"></i>
                </button>
            </div>`;
    }
    
    col.innerHTML = `
        <div class="dark-theme-card h-100 clickable-card" onclick="navigateToTripDetails(${tripId})" style="cursor: pointer; transition: all 0.3s ease;">
            <div class="position-relative">
                ${imageHtml}
                ${priceBadgeHtml}
            </div>
            <div class="card-body">
                <h5 class="card-title fw-bold" style="color: #3498db;">${trip.title || trip.Title}</h5>
                <h6 class="card-subtitle mb-3" style="color: #2ecc71;">
                    <i class="fas fa-map-marker-alt me-1"></i>${trip.destinationName || trip.DestinationName}
                </h6>
                <p class="card-text mb-3" style="color: #bdc3c7; line-height: 1.5;">${truncatedDescription}</p>
                
                <!-- Trip Info Badges -->
                <div class="row g-2 mb-3">
                    <div class="col-12">
                        <div class="d-flex flex-wrap gap-2">
                            <span class="badge dark-theme-badge badge-warning">
                                <i class="fas fa-calendar-alt me-1"></i>${startDate} - ${endDate}
                            </span>
                            <span class="badge dark-theme-badge badge-info">
                                <i class="fas fa-clock me-1"></i>${trip.durationInDays || trip.DurationInDays} days
                            </span>
                        </div>
                    </div>
                    <div class="col-12">
                        ${isAvailable ? 
                            `<span class="badge dark-theme-badge badge-success">
                                <i class="fas fa-users me-1"></i>${availableSlots} spots left
                            </span>` :
                            `<span class="badge dark-theme-badge badge-danger">
                                <i class="fas fa-exclamation-circle me-1"></i>Fully booked
                            </span>`
                        }
                    </div>
                </div>
            </div>
            
            <div class="card-footer" style="background: rgba(255, 255, 255, 0.02); border-top: 1px solid rgba(255, 255, 255, 0.1);">
                <div class="d-flex gap-2 flex-wrap">
                    ${actionButtons}
                </div>
            </div>
        </div>`;
    
    return col;
}

// Update pagination controls
function updatePagination(pagination) {
    const paginationContainer = document.querySelector('.pagination');
    if (!paginationContainer) return;
    
    const nav = paginationContainer.closest('nav');
    if (!nav) return;
    
    // Clear existing pagination
    paginationContainer.innerHTML = '';
    
    // Previous button
    const prevClass = pagination.hasPreviousPage ? '' : 'disabled';
    const prevButton = document.createElement('li');
    prevButton.className = `page-item ${prevClass}`;
    
    if (pagination.hasPreviousPage) {
        prevButton.innerHTML = `
            <a class="page-link" href="?page=${pagination.currentPage - 1}&destinationId=${currentDestinationId || ''}">
                <i class="fas fa-chevron-left me-1"></i> Previous
            </a>`;
    } else {
        prevButton.innerHTML = `
            <span class="page-link">
                <i class="fas fa-chevron-left me-1"></i> Previous
            </span>`;
    }
    paginationContainer.appendChild(prevButton);
    
    // Page numbers
    const startPage = Math.max(1, pagination.currentPage - 2);
    const endPage = Math.min(pagination.totalPages, pagination.currentPage + 2);
    
    // First page + ellipsis if needed
    if (startPage > 1) {
        const firstPage = createPageButton(1, pagination.currentPage);
        paginationContainer.appendChild(firstPage);
        
        if (startPage > 2) {
            const ellipsis = document.createElement('li');
            ellipsis.className = 'page-item disabled';
            ellipsis.innerHTML = '<span class="page-link">...</span>';
            paginationContainer.appendChild(ellipsis);
        }
    }
    
    // Page range
    for (let i = startPage; i <= endPage; i++) {
        const pageButton = createPageButton(i, pagination.currentPage);
        paginationContainer.appendChild(pageButton);
    }
    
    // Last page + ellipsis if needed
    if (endPage < pagination.totalPages) {
        if (endPage < pagination.totalPages - 1) {
            const ellipsis = document.createElement('li');
            ellipsis.className = 'page-item disabled';
            ellipsis.innerHTML = '<span class="page-link">...</span>';
            paginationContainer.appendChild(ellipsis);
        }
        
        const lastPage = createPageButton(pagination.totalPages, pagination.currentPage);
        paginationContainer.appendChild(lastPage);
    }
    
    // Next button
    const nextClass = pagination.hasNextPage ? '' : 'disabled';
    const nextButton = document.createElement('li');
    nextButton.className = `page-item ${nextClass}`;
    
    if (pagination.hasNextPage) {
        nextButton.innerHTML = `
            <a class="page-link" href="?page=${pagination.currentPage + 1}&destinationId=${currentDestinationId || ''}">
                Next <i class="fas fa-chevron-right ms-1"></i>
            </a>`;
    } else {
        nextButton.innerHTML = `
            <span class="page-link">
                Next <i class="fas fa-chevron-right ms-1"></i>
            </span>`;
    }
    paginationContainer.appendChild(nextButton);
    
    // Update pagination info
    updatePaginationInfo(pagination);
}

// Create a page button
function createPageButton(pageNumber, currentPage) {
    const li = document.createElement('li');
    const isActive = pageNumber === currentPage;
    li.className = `page-item ${isActive ? 'active' : ''}`;
    
    if (isActive) {
        li.innerHTML = `<span class="page-link">${pageNumber}</span>`;
    } else {
        li.innerHTML = `
            <a class="page-link" href="?page=${pageNumber}&destinationId=${currentDestinationId || ''}">
                ${pageNumber}
            </a>`;
    }
    
    return li;
}

// Update pagination info text
function updatePaginationInfo(pagination) {
    const infoElement = document.querySelector('.text-center.mt-2 small');
    if (infoElement) {
        infoElement.innerHTML = `
            Showing ${pagination.startItem} to ${pagination.endItem} of ${pagination.totalItems} trips
        `;
    }
}

// Update browser URL without page reload
function updateUrl() {
    const params = new URLSearchParams();
    params.append('page', currentPage.toString());
    
    if (currentDestinationId) {
        params.append('destinationId', currentDestinationId);
    }
    
    const newUrl = `${window.location.pathname}?${params.toString()}`;
    window.history.pushState(null, '', newUrl);
}

// Add loading indicator
function addLoadingIndicator() {
    if (document.getElementById('tripLoadingIndicator')) return;
    
    const container = document.querySelector('.container.mt-4');
    if (!container) return;
    
    const loadingDiv = document.createElement('div');
    loadingDiv.id = 'tripLoadingIndicator';
    loadingDiv.className = 'text-center my-4 d-none';
    loadingDiv.innerHTML = `
        <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading trips...</span>
        </div>
        <p class="mt-2 text-muted">Loading trips...</p>
    `;
    
    container.insertBefore(loadingDiv, container.children[2]);
}

// Show loading indicator
function showLoadingIndicator() {
    const indicator = document.getElementById('tripLoadingIndicator');
    if (indicator) {
        indicator.classList.remove('d-none');
    }
}

// Hide loading indicator
function hideLoadingIndicator() {
    const indicator = document.getElementById('tripLoadingIndicator');
    if (indicator) {
        indicator.classList.add('d-none');
    }
}

// Show no trips message
function showNoTripsMessage() {
    const container = document.querySelector('.row.row-cols-1.row-cols-md-3.g-4');
    if (!container) return;
    
    const messageDiv = document.createElement('div');
    messageDiv.className = 'col-12';
    messageDiv.innerHTML = `
        <div class="alert alert-info" role="alert">
            <i class="fas fa-info-circle me-2"></i> No trips found. Please try a different filter or check back later.
        </div>
    `;
    
    container.appendChild(messageDiv);
}

// Show message with timeout
function showMessage(message, type) {
    // Remove any existing messages
    const existingAlert = document.querySelector('.custom-alert');
    if (existingAlert) {
        existingAlert.remove();
    }
    
    // Create new alert
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type === 'error' ? 'danger' : 'success'} alert-dismissible fade show custom-alert`;
    alertDiv.style.position = 'fixed';
    alertDiv.style.top = '20px';
    alertDiv.style.right = '20px';
    alertDiv.style.zIndex = '9999';
    alertDiv.style.minWidth = '300px';
    
    const icon = type === 'error' ? 'fas fa-exclamation-triangle' : 'fas fa-check-circle';
    
    alertDiv.innerHTML = `
        <i class="${icon} me-2"></i>${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    document.body.appendChild(alertDiv);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (alertDiv && alertDiv.parentNode) {
            alertDiv.remove();
        }
    }, 5000);
}

// Admin function to populate trip images
async function populateTripImages() {
    const button = event.target;
    const originalText = button.innerHTML;
    
    try {
        // Show loading state
        button.disabled = true;
        button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Populating Images...';
        
        showMessage('Starting image population process...', 'info');
        
        const response = await fetch('/api/unsplash/populate-trip-images', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const result = await response.json();
        
        // Show detailed results
        const successCount = result.results ? result.results.filter(r => r.status && r.status.includes('SUCCESS')).length : 0;
        const errorCount = result.results ? result.results.filter(r => r.status && (r.status.includes('ERROR') || r.status.includes('EXCEPTION'))).length : 0;
        const alreadyHasCount = result.results ? result.results.filter(r => r.status && r.status.includes('ALREADY_HAS_IMAGE')).length : 0;
        
        let message = `Image population completed! `;
        message += `✅ ${successCount} images added, `;
        message += `⏭️ ${alreadyHasCount} already had images, `;
        message += `❌ ${errorCount} failed`;
        
        showMessage(message, successCount > 0 ? 'success' : 'info');
        
        // Reload the page to show new images
        if (successCount > 0) {
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        }
        
    } catch (error) {
        console.error('Error populating images:', error);
        showMessage(`Error populating images: ${error.message}`, 'error');
    } finally {
        // Restore button state
        button.disabled = false;
        button.innerHTML = originalText;
    }
}

// Admin function to refresh all trip images
async function refreshAllImages() {
    const button = event.target;
    const originalText = button.innerHTML;
    
    // Confirm action
    if (!confirm('This will refresh images for ALL trips. Are you sure?')) {
        return;
    }
    
    try {
        // Show loading state
        button.disabled = true;
        button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Refreshing All Images...';
        
        showMessage('Starting image refresh for all trips...', 'info');
        
        const response = await fetch('/api/unsplash/force-refresh-images', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const result = await response.json();
        
        // Show results
        const successCount = result.results ? result.results.filter(r => r.status && r.status.includes('SUCCESS')).length : 0;
        const errorCount = result.results ? result.results.filter(r => r.status && (r.status.includes('ERROR') || r.status.includes('EXCEPTION'))).length : 0;
        
        let message = `Image refresh completed! `;
        message += `✅ ${successCount} images refreshed, `;
        message += `❌ ${errorCount} failed`;
        
        showMessage(message, successCount > 0 ? 'success' : 'info');
        
        // Reload the page to show new images
        if (successCount > 0) {
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        }
        
    } catch (error) {
        console.error('Error refreshing images:', error);
        showMessage(`Error refreshing images: ${error.message}`, 'error');
    } finally {
        // Restore button state
        button.disabled = false;
        button.innerHTML = originalText;
    }
}

// Initialize enhanced hover effects for clickable cards
function initializeCardHoverEffects() {
    const clickableCards = document.querySelectorAll('.clickable-card');
    
    clickableCards.forEach(card => {
        // Remove any existing event listeners to prevent duplicates
        card.removeEventListener('mouseenter', cardMouseEnter);
        card.removeEventListener('mouseleave', cardMouseLeave);
        card.removeEventListener('mousedown', cardMouseDown);
        card.removeEventListener('mouseup', cardMouseUp);
        
        // Add event listeners
        card.addEventListener('mouseenter', cardMouseEnter);
        card.addEventListener('mouseleave', cardMouseLeave);
        card.addEventListener('mousedown', cardMouseDown);
        card.addEventListener('mouseup', cardMouseUp);
    });
}

// Card hover event handlers
function cardMouseEnter() {
    this.style.transform = 'translateY(-8px) scale(1.02)';
    this.style.boxShadow = '0 20px 40px rgba(0, 0, 0, 0.6), 0 0 0 1px rgba(52, 152, 219, 0.3)';
}

function cardMouseLeave() {
    this.style.transform = 'translateY(0) scale(1)';
    this.style.boxShadow = '';
}

function cardMouseDown() {
    this.style.transform = 'translateY(-4px) scale(1.01)';
}

function cardMouseUp() {
    this.style.transform = 'translateY(-8px) scale(1.02)';
} 