function showPage() {
  if (!document.querySelector('#dashboard.page-section')) return;
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

const shortener = document.querySelector('[data-demo-shortener]');
shortener?.addEventListener('submit', event => {
  event.preventDefault();
  let notice = document.querySelector('#demo-shortener-notice');
  if (!notice) {
    notice = document.createElement('p');
    notice.id = 'demo-shortener-notice';
    notice.style.cssText = 'margin:12px 0;color:#243b61;font-weight:600;line-height:1.5';
    shortener.after(notice);
  }
  notice.innerHTML = 'This is a static portfolio preview. <a href="dashboard.html#links">Explore the dashboard</a> to see the link management interface.';
});

for (const [openId, closeId, modalId] of [
  ['ContactBtn', 'closeModal', 'contactModal'],
  ['ViolationReportBtn', 'closeViolationReportModal', 'violationReportModal']
]) {
  const modal = document.getElementById(modalId);
  document.getElementById(openId)?.addEventListener('click', event => {
    event.preventDefault();
    modal?.classList.add('active');
  });
  document.getElementById(closeId)?.addEventListener('click', () => modal?.classList.remove('active'));
  modal?.addEventListener('click', event => {
    if (event.target === modal) modal.classList.remove('active');
  });
}

if (document.querySelector('.blog-search')) {
  const params = new URLSearchParams(location.search);
  const query = (params.get('q') || '').trim().toLowerCase();
  const category = (params.get('category') || '').trim().toLowerCase();
  const input = document.querySelector('.blog-search input[name="q"]');
  if (input) input.value = params.get('q') || '';
  document.querySelectorAll('.article-card').forEach(card => {
    const text = card.textContent.toLowerCase();
    const cardCategory = card.querySelector('.card-category, .article-meta span:last-child')?.textContent.toLowerCase() || '';
    card.hidden = Boolean((query && !text.includes(query)) || (category && !cardCategory.includes(category)));
  });
  const featured = document.querySelector('.featured-article');
  if (featured) {
    const text = featured.textContent.toLowerCase();
    const featuredCategory = featured.querySelector('.article-meta span')?.textContent.toLowerCase() || '';
    featured.hidden = Boolean((query && !text.includes(query)) || (category && !featuredCategory.includes(category)));
  }
}
