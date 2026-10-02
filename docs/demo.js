function showPage() {
  const page = (location.hash.slice(1).split('-')[0] || 'dashboard');
  const target = ['dashboard', 'links', 'create', 'analytics', 'profile'].includes(page) ? page : 'dashboard';
  document.querySelectorAll('.page-section').forEach(section => {
    section.classList.toggle('active', section.id === target);
  });
  document.querySelectorAll('[data-page]').forEach(link => {
    link.classList.toggle('active', link.dataset.page === target);
  });
  window.scrollTo({ top: 0, behavior: 'instant' });
}
window.addEventListener('hashchange', showPage);
showPage();
document.querySelectorAll('[data-demo-form]').forEach(form => {
  form.addEventListener('submit', event => event.preventDefault());
});
