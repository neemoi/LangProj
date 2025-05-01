import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Container, Button, Alert, Spinner } from 'react-bootstrap';
import { ProfileEdit, ProfileView } from '../../components/userprofile';
import Navigation from '../../components/layout/Navigation/Navigation';
import './ProfilePage.css';

const ProfilePage = () => {
  const [userData, setUserData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [activeTab, setActiveTab] = useState('view');

  const navigate = useNavigate();

  useEffect(() => {
    const fetchUserProfile = async () => {
      const userToken = localStorage.getItem('userToken');
      const currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
      const userId = currentUser?.id || currentUser?.userId;

      if (!currentUser || !userToken || !userId) {
        navigate('/login', { replace: true });
        return;
      }

      try {
        const response = await fetch(`http://localhost:5000/api/users/${userId}`, {
          headers: {
            'Authorization': `Bearer ${userToken}`,
            'Content-Type': 'application/json',
          },
        });

        if (!response.ok) {
          if (response.status === 401) {
            localStorage.removeItem('currentUser');
            localStorage.removeItem('userToken');
            navigate('/login');
            return;
          }
          throw new Error(`Ошибка HTTP! Статус: ${response.status}`);
        }

        const data = await response.json();
        setUserData(data);
      } catch (err) {
        console.error('Ошибка загрузки профиля:', err);
        setError(err.message || 'Не удалось загрузить данные профиля');
      } finally {
        setLoading(false);
      }
    };

    fetchUserProfile();
  }, [navigate]);

  const handleLogout = () => {
    localStorage.removeItem('currentUser');
    localStorage.removeItem('userToken');
    navigate('/login');
  };

  const handleTabChange = (tab) => {
    setActiveTab(tab);
  };

  const currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
  const userId = currentUser?.id || currentUser?.userId;

  if (loading) {
    return (
      <div className="profile-loading-screen">
        <Navigation 
          currentUser={currentUser}
          onLogout={handleLogout}
          onShowLogin={() => navigate('/login')}
          onShowRegister={() => navigate('/register')}
        />
        <Container className="text-center mt-5">
          <Spinner animation="border" variant="primary" />
          <p className="mt-3 text-muted">Загружаем ваш профиль...</p>
        </Container>
      </div>
    );
  }

  if (error) {
    return (
      <div>
        <Navigation 
          currentUser={currentUser}
          onLogout={handleLogout}
          onShowLogin={() => navigate('/login')}
          onShowRegister={() => navigate('/register')}
        />
        <Container className="mt-4">
          <Alert variant="danger" className="shadow">
            <Alert.Heading>Ошибка!</Alert.Heading>
            <p>{error}</p>
            <Button 
              variant="outline-danger" 
              onClick={() => window.location.reload()}
            >
              Попробовать снова
            </Button>
          </Alert>
        </Container>
      </div>
    );
  }

  if (!userData) {
    return (
      <div>
        <Navigation 
          currentUser={currentUser}
          onLogout={handleLogout}
          onShowLogin={() => navigate('/login')}
          onShowRegister={() => navigate('/register')}
        />
        <Container className="mt-4">
          <Alert variant="warning" className="shadow">
            Данные профиля не найдены
          </Alert>
        </Container>
      </div>
    );
  }

  return (
    <div className="profile-page">
      <Navigation 
        currentUser={currentUser}
        onLogout={handleLogout}
        onShowLogin={() => navigate('/login')}
        onShowRegister={() => navigate('/register')}
      />

      <Container className="profile-container py-4">
        <div className="profile-header bg-white p-4 rounded-3 shadow-sm mb-4">
          <div className="d-flex flex-column flex-md-row justify-content-between align-items-center">
            <div className="text-center text-md-start mb-3 mb-md-0">
              <h2 className="mb-1 fw-bold text-gradient">
                {userData.username || userData.userName}
              </h2>
              <p className="text-muted mb-0">{userData.email}</p>
            </div>

            <div className="d-flex flex-wrap justify-content-center gap-2">
              <Button 
                variant={activeTab === 'view' ? 'primary' : 'outline-secondary'} 
                onClick={() => handleTabChange('view')}
                className="px-4 rounded-dark"
              >
                <i className="bi bi-person-lines-fill me-2"></i>
                Просмотр
              </Button>
              <Button 
                variant={activeTab === 'edit' ? 'primary' : 'outline-secondary'} 
                onClick={() => handleTabChange('edit')}
                className="px-4 rounded-dark"
              >
                <i className="bi bi-pencil-square me-2"></i>
                Редактировать
              </Button>
            </div>
          </div>
        </div>

        <div className="profile-content bg-white p-4 rounded-3 shadow-sm">
          {activeTab === 'view' ? (
            <ProfileView userData={userData} />
          ) : (
            <ProfileEdit 
              key={userId}
              userData={userData}
              setUserData={setUserData}
              setError={setError}
              currentUserId={userId}
              onCancel={() => handleTabChange('view')}
            />
          )}
        </div>
      </Container>
    </div>
  );
};

export default ProfilePage;
