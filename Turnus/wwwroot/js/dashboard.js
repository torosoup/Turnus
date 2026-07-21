window.Dashboard = {

    changeVenue: function (venueId) {
        const url = `/Admin/Dashboard?venueId=${venueId}`;
        window.location.href = url;
    },

    changeDepartment: function (departmentId) {
        const venueId = document.getElementById("venueSelect").value;

        const url = `/Admin/Dashboard?venueId=${venueId}&departmentId=${departmentId}`;
        window.location.href = url;
    },

    loadSections: function (venueId, departmentId) {
        $("#venue-settings-section").load(`/Admin/VenueSettingsSection?venueId=${venueId}`);
        $("#departments-section").load(`/Admin/DepartmentsSection?venueId=${venueId}`);
        $("#roles-section").load(`/Admin/RolesSection?venueId=${venueId}&departmentId=${departmentId}`);
        $("#shift-definitions-section").load(`/Admin/ShiftDefinitionsSection?venueId=${venueId}&departmentId=${departmentId}`);
        $("#staffing-requirements-section").load(`/Admin/StaffingRequirementsSection?venueId=${venueId}&departmentId=${departmentId}`);
        $("#schedule-section").load(`/Admin/ScheduleSection?venueId=${venueId}&departmentId=${departmentId}`);
    }
};