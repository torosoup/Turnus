window.Dashboard = {

    changeVenue: function (venueId) {
        const section = Dashboard._getCurrentSection();
        const url = `/Admin/Dashboard?venueId=${venueId}` + (section ? `&section=${section}` : '');
        window.location.href = url;
    },

    changeDepartment: function (departmentId) {
        const venueId = document.getElementById("venueSelect").value;
        const section = Dashboard._getCurrentSection();

        const url = `/Admin/Dashboard?venueId=${venueId}&departmentId=${departmentId}` + (section ? `&section=${section}` : '');
        window.location.href = url;
    },

    loadSections: function (venueId, departmentId) {
        $("#venue-settings-section").load(`/Admin/VenueSettingsSection?venueId=${venueId}`);
        $("#departments-section").load(`/Admin/DepartmentsSection?venueId=${venueId}`);
        $("#roles-section").load(`/Admin/RolesSection?venueId=${venueId}&departmentId=${departmentId}`);
        $("#shift-definitions-section").load(`/Admin/ShiftDefinitionsSection?venueId=${venueId}&departmentId=${departmentId}`);
        $("#staffing-requirements-section").load(`/Admin/StaffingRequirementsSection?venueId=${venueId}&departmentId=${departmentId}`);
        // UsersSection is universal; it does not require venueId or departmentId
        $("#user-management-section").load(`/Admin/UsersSection`, function () {
            // show the section only if the user is currently viewing 'users'
            const usersNode = document.getElementById('user-management-section');
            if (Dashboard._getCurrentSection() === 'users') {
                if (usersNode) usersNode.style.display = '';
            } else {
                if (usersNode) usersNode.style.display = 'none';
            }
        });
        $("#schedule-section").load(`/Admin/ScheduleSection?venueId=${venueId}&departmentId=${departmentId}`);
    }
};

// helper to determine current active section
Dashboard._getCurrentSection = function () {
    const configLink = document.getElementById('sidebar-link-configuration');
    const scheduleLink = document.getElementById('sidebar-link-schedule');
    const usersLink = document.getElementById('sidebar-link-users');

    if (configLink && configLink.classList.contains('active')) return 'configuration';
    if (scheduleLink && scheduleLink.classList.contains('active')) return 'schedule';
    if (usersLink && usersLink.classList.contains('active')) return 'users';

    // fallback: check visible sections
    const scheduleSection = document.getElementById('schedule-section-wrapper');
    const usersSection = document.getElementById('user-management-section');
    const configSection = document.getElementById('configuration-section');

    if (scheduleSection && scheduleSection.style.display !== 'none') return 'schedule';
    if (usersSection && usersSection.style.display !== 'none') return 'users';
    if (configSection && configSection.style.display !== 'none') return 'configuration';

    return 'configuration';
};

// UI helpers to switch visible sections inside the admin dashboard without navigating away
Dashboard.showConfiguration = function () {
    const config = document.getElementById('configuration-section');
    const schedule = document.getElementById('schedule-section-wrapper');
    const users = document.getElementById('user-management-section');

    if (config) config.style.display = '';
    if (schedule) schedule.style.display = 'none';
    if (users) users.style.display = 'none';

    Dashboard._setActiveSidebar('configuration');
};

Dashboard.showSchedule = function () {
    const config = document.getElementById('configuration-section');
    const schedule = document.getElementById('schedule-section-wrapper');
    const users = document.getElementById('user-management-section');

    if (config) config.style.display = 'none';
    if (schedule) schedule.style.display = '';
    if (users) users.style.display = 'none';

    Dashboard._setActiveSidebar('schedule');
};

Dashboard.showUsers = function () {
    const config = document.getElementById('configuration-section');
    const schedule = document.getElementById('schedule-section-wrapper');
    const users = document.getElementById('user-management-section');

    if (config) config.style.display = 'none';
    if (schedule) schedule.style.display = 'none';
    if (users) users.style.display = '';

    Dashboard._setActiveSidebar('users');
};

Dashboard._setActiveSidebar = function (key) {
    // clear existing
    const links = [
        document.getElementById('sidebar-link-configuration'),
        document.getElementById('sidebar-link-schedule'),
        document.getElementById('sidebar-link-users')
    ];

    links.forEach(l => { if (l) l.classList.remove('active'); });

    const target = document.getElementById('sidebar-link-' + key);
    if (target) target.classList.add('active');
};

// helper to read query string parameter
function _getQueryParam(name) {
    const params = new URLSearchParams(window.location.search);
    return params.get(name);
}

// Restore selected section from query param on page load
document.addEventListener('DOMContentLoaded', function () {
    const section = _getQueryParam('section');
    if (section === 'schedule') {
        Dashboard.showSchedule();
    } else if (section === 'users') {
        Dashboard.showUsers();
    } else {
        Dashboard.showConfiguration();
    }
});