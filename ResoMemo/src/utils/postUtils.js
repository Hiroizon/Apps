export const readPosts = () => {
    const posts = localStorage.getItem('posts');
    return posts ? JSON.parse(posts) : [];
  };
  
  export const writePosts = posts => {
    localStorage.setItem('posts', JSON.stringify(posts));
  };