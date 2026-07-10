window.Dashboard = {
    changeVenue: function (venueId) {
        const url = `/Admin/Dashboard?venueId=${venueId}`;
        window.location.href = url;
    },

    loadSections: function (venueId) {
        $("#venue-settings-section").load(`/Admin/VenueSettingsSection?venueId=${venueId}`);
        $("#departments-section").load(`/Admin/DepartmentsSection?venueId=${venueId}`);
        $("#roles-section").load(`/Admin/RolesSection?venueId=${venueId}`);
        $("#shift-definitions-section").load(`/Admin/ShiftDefinitionsSection?venueId=${venueId}`);
        $("#staffing-requirements-section").load(`/Admin/StaffingRequirementsSection?venueId=${venueId}`);
        $("#schedule-section").load(`/Admin/ScheduleSection?venueId=${venueId}`);
    }
};
